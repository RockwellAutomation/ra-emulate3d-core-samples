//==============================================================================
/// MIT License  
///  
/// Copyright (c) 2026 Rockwell Automation, Inc.  
///  
/// Permission is hereby granted, free of charge, to any person obtaining a copy  
/// of this software and associated documentation files (the "Software"), to deal  
/// in the Software without restriction, including without limitation the rights  
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell  
/// copies of the Software, and to permit persons to whom the Software is  
/// furnished to do so, subject to the following conditions:  
///  
/// The above copyright notice and this permission notice shall be included in all  
/// copies or substantial portions of the Software.  
///  
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR  
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,  
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE  
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER  
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,  
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE  
/// SOFTWARE.  
///  
//==============================================================================

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Demo3D.Mcp;
using Demo3D.Visuals;
using ModelContextProtocol.Server;

namespace Demo3D.Components {
    /// <summary>
    /// The state of one conveyor, as returned to MCP clients.
    /// </summary>
    public record ConveyorStatus(string Name, double Length, bool IsMotorOn, double MotorSpeed);

    /// <summary>
    /// Custom MCP tools that read and control the conveyor line in this model.
    /// </summary>
    [McpServerToolType]
    [Description("Tools for reading and controlling the conveyor line in this model.")]
    public class ConveyorLineTools(McpPermissionHolder permissions) : McpToolBase(permissions) {
        const double MaxSpeed = 2;

        [McpServerTool]
        [Description("Get every conveyor in the line, in flow order, with its length, motor state and motor speed.")]
        public McpResult<List<ConveyorStatus>> GetLineStatus() {
            return ExecuteMcpCommand(McpPermission.AllowSceneRead, app => {
                var conveyors = OrderByFlow(app.Document.Visuals.OfType<ConveyorVisual>().ToList());
                if (conveyors.Count == 0) {
                    return Error<List<ConveyorStatus>>("The model has no conveyors.");
                }
                return Success(conveyors.Select(ToStatus).ToList());
            });
        }

        [McpServerTool]
        [Description("Set the motor speed of a conveyor, in metres per second.")]
        public McpResult<ConveyorStatus> SetConveyorSpeed(
            [Description("The name of the conveyor, such as CV_01.")] string conveyorName,
            [Description("The new motor speed in metres per second, from 0 to 2.")] double speed) {
            return ExecuteMcpCommand(McpPermission.AllowSceneModification, app => {
                if (speed < 0 || speed > MaxSpeed) {
                    return Error<ConveyorStatus>($"Speed {speed} is outside the allowed range of 0 to {MaxSpeed} m/s.");
                }
                if (app.Document.FindVisual(conveyorName) is not ConveyorVisual conveyor) {
                    return Error<ConveyorStatus>($"Conveyor '{conveyorName}' not found.");
                }
                conveyor.MotorSpeed = speed;
                return Success(ToStatus(conveyor));
            });
        }

        [McpServerTool]
        [Description("Turn a conveyor's motor on or off.")]
        public McpResult<ConveyorStatus> SetConveyorMotor(
            [Description("The name of the conveyor, such as CV_01.")] string conveyorName,
            [Description("True to turn the motor on, false to turn it off.")] bool isMotorOn) {
            return ExecuteMcpCommandWithRedraw(McpPermission.AllowSceneModification, app => {
                if (app.Document.FindVisual(conveyorName) is not ConveyorVisual conveyor) {
                    return Error<ConveyorStatus>($"Conveyor '{conveyorName}' not found.");
                }
                if (isMotorOn) {
                    conveyor.MotorOn();
                }
                else {
                    conveyor.MotorOff();
                }
                return Success(ToStatus(conveyor));
            });
        }

        static McpResult<T> Success<T>(T result) => new() { Result = result };

        static McpResult<T> Error<T>(string message) => new() { ErrorMessage = message };

        static ConveyorStatus ToStatus(ConveyorVisual conveyor) =>
            new(conveyor.Name, conveyor.Length, conveyor.IsMotorOn, conveyor.MotorSpeed);

        /// <summary>
        /// Orders conveyors by following each chain from its first conveyor through <see cref="ConveyorVisual.Next"/>.
        /// </summary>
        static List<ConveyorVisual> OrderByFlow(List<ConveyorVisual> conveyors) {
            var ordered = new List<ConveyorVisual>();
            var visited = new HashSet<ConveyorVisual>();
            var heads = conveyors.Where(c => c.Prev == null || !conveyors.Contains(c.Prev));
            foreach (var head in heads.Concat(conveyors)) {
                for (var c = head; c != null && visited.Add(c); c = c.Next) {
                    ordered.Add(c);
                }
            }
            return ordered;
        }
    }

    /// <summary>
    /// Exposes the skills deployed alongside this script's assembly to MCP clients.
    /// </summary>
    public static class ConveyorLineSkills {
        static readonly string AssemblyDir =
            Path.GetDirectoryName(typeof(ConveyorLineSkills).Assembly.Location) ?? string.Empty;

        /// <summary>
        /// Returns the folder that holds one sub-folder per skill, each containing a SKILL.md.
        /// </summary>
        [McpSkillProvider]
        public static string GetSkillsFolder() {
            return Path.Combine(AssemblyDir, "Skills");
        }
    }
}
