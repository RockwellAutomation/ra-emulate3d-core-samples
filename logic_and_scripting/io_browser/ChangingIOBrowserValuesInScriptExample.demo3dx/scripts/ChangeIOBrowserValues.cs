//==============================================================================  
/// MIT License  
///  
/// Copyright (c) 2025 Rockwell Automation, Inc.  
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

#region Namespaces
using Demo3D.Native;
using Demo3D.TagServer;
using Demo3D.Visuals;
using System.ComponentModel;
using System.Linq;
#endregion

namespace Demo3D.Components {
    [Auto] public class ChangeIOBroswerValues {

        [Auto] static protected Document document;
        [Auto] static protected PrintDelegate print;

        [Auto, Category("-Force/Unforce IO Values"), Description("Set the value of the O_LAMP1 tag server value via script.")]
        SimplePropertyValue<bool> I_TestBool_ForceValue;
        [Auto, Category("-Force/Unforce IO Values"), Description("Unforce all IO Browser bindings.")]
        SimplePropertyValue<bool> UnForceAllBindings;
        [Auto, Category("-Print Info"), Description("Print all binding info.")]
        SimplePropertyValue<bool> PrintAllBindingInfo;
        [Auto, Category("-Print Info"), Description("Print all IO Browser Info.")]
        SimplePropertyValue<bool> PrintIOBrowserInfo;

        #region Set/Unset IO Browser Forced Values
        [Auto] void OnUnForceAllBindingsUpdated(Visual sender, bool value, bool oldValue) {
            if (value) {
                document.NeedsSave = true;
                foreach (var binding in document.Connections.Bindings) {
                    binding.Forced = false;
                }
            }
            UnForceAllBindings.Value = false;
        }

        [Auto] void OnI_TestBool_ForceValueUpdated(Visual sender, bool value, bool oldValue) {
            BindableItem bi_testboolin = FindBindableItem("TestBool_In", document.FindVisual("Box1"));
            bi_testboolin.Force(value);
        }

        private static BindableItem FindBindableItem(string bindingName, Visual visual) {
            foreach (Visual v in document.Scene.Descendants) {
                foreach (BindableItem item in v.FindBindableItems()) {
                    if (item.BindingName == bindingName && item.Visual == visual) return item;
                }
            }
            return null;
        }
        #endregion

        #region Print IO Browser and All Binding Info to Message Log
        [Auto] void OnPrintIOBrowserInfoUpdated(Visual sender, bool value, bool oldValue) {
            if (value) {
                document.NeedsSave = true;
                foreach (var binding in document.Connections.Bindings) {
                    print($"\nIO Browser Info | {binding}");
                    print("  L Active: ".PadRight(20) + binding.Active.ToString().PadRight(40) + " (get/set)");
                    print("  L Address: ".PadRight(20) + binding.ServerItem.Path.ToString().PadRight(40) + " (get/set)");
                    print("  L Name: ".PadRight(20) + binding.ServerItem.ItemID.ToString().PadRight(40) + " (get)");
                    print("  L Type: ".PadRight(20) + binding.ServerItem.DataType.ToString().PadRight(40) + " (get/set)");
                    print("  L Server: ".PadRight(20) + binding.ServerItem.Server.ServerName.ToString().PadRight(40) + " (get)");
                    print("  L Server Value: ".PadRight(20) + binding.ServerItem.Value.ToString().PadRight(40) + " (get)");
                    print("  L Model Value: ".PadRight(20) + binding.Expression.BindableItems.First().Value.ToString().PadRight(40) + " (get/set)");
                    string description = (binding.ServerItem[PropertyId.ItemDescription] is not null) ? binding.ServerItem[PropertyId.ItemDescription].ToString() : "";
                    print("  L Description: ".PadRight(20) + description.PadRight(40) + " (get/set)");
                    print("  L Access: ".PadRight(20) + binding.ServerItem.AccessRights.ToString().PadRight(40) + " (get/set)");
                    print("  L Scan Rate: ".PadRight(20) + binding.ServerItem.ScanRate.ToString().PadRight(40) + " (get/set)");
                    string displayName = (binding.ServerItem[PropertyId.DisplayName] is not null) ? binding.ServerItem[PropertyId.DisplayName].ToString() : "";
                    print("  L Display Name: ".PadRight(20) + displayName.PadRight(40) + " (get/set)");
                    print("  L Visual: ".PadRight(20) + binding.Expression.BindableItems.First().Visual.Name.ToString().PadRight(40) + " (get/set)");
                    print("  L Property: ".PadRight(20) + binding.Expression.BindableItems.First().PropertyName.ToString().PadRight(40) + " (get)");
                    string expression = (binding.Expression.TagExpression.ExpressionType == TagExpressionType.Formula) ? binding.Expression.TagExpression.Expression : "";
                    print("  L Expression: ".PadRight(20) + expression.PadRight(40) + " (get)");
                }
            }
            PrintIOBrowserInfo.Value = false;
        }

        [Auto] void OnPrintAllBindingInfoUpdated(Visual sender, bool value, bool oldValue) {
            if (value) {
                document.NeedsSave = true;
                foreach (var binding in document.Connections.Bindings) {
                    print($"\nBinding info for {binding}");
                    print("  L ServerItem: " + binding.ServerItem);
                    print("  |   L AccessRights: " + binding.ServerItem.AccessRights);
                    print("  |   L AllowedAccess: " + binding.ServerItem.AllowedAccess);
                    print("  |   L DataType: " + binding.ServerItem.DataType);
                    print("  |   L IOControl: " + binding.ServerItem.IOControl);
                    print("  |   L ItemID: " + binding.ServerItem.ItemID);
                    print("  |   L Path: " + binding.ServerItem.Path);
                    print("  |   L Properties: " + binding.ServerItem.Properties);
                    print("  |   L ScanRate: " + binding.ServerItem.ScanRate);
                    print("  |   L ServerName: " + binding.ServerItem.ServerName);
                    print("  |   L Value: " + binding.ServerItem.Value);
                    print("  |   L DataValue: " + binding.ServerItem.DataValue);
                    print("  |   --- ");
                    print("  |   L DataType: " + binding.ServerItem[PropertyId.DataType]);
                    print("  |   L Value: " + binding.ServerItem[PropertyId.Value]);
                    print("  |   L Quality: " + binding.ServerItem[PropertyId.Quality]);
                    print("  |   L Timestamp: " + binding.ServerItem[PropertyId.Timestamp]);
                    print("  |   L AccessRights: " + binding.ServerItem[PropertyId.AccessRights]);
                    print("  |   L ScanRate: " + binding.ServerItem[PropertyId.ScanRate]);
                    print("  |   L DisplayName: " + binding.ServerItem[PropertyId.DisplayName]);
                    print("  |   L EUUnits: " + binding.ServerItem[PropertyId.EUUnits]);
                    print("  |   L ItemDescription: " + binding.ServerItem[PropertyId.ItemDescription]);
                    print("  L Expression: " + binding.Expression);
                    print("  |   L TagExpression: " + binding.Expression.TagExpression);
                    print("  |   |   L ExpressionType: " + binding.Expression.TagExpression.ExpressionType);
                    print("  |   |   L Expression: " + binding.Expression.TagExpression.Expression);
                    print("  |   L Active: " + binding.Expression.Active);
                    print("  |   L BindableItems: " + binding.Expression.BindableItems);
                    var bindableItemList = binding.Expression.BindableItems.ToList();
                    foreach (var bindableItem in bindableItemList) {
                        print($"  |   |   L {bindableItem} AllowedAccess: " + bindableItem.AllowedAccess);
                        print($"  |   |   L {bindableItem} BindingInterface: " + bindableItem.BindingInterface);
                        print($"  |   |   L {bindableItem} DefaultAccess: " + bindableItem.DefaultAccess);
                        print($"  |   |   L {bindableItem} Expression: " + bindableItem.Expression);
                        print($"  |   |   L {bindableItem} HasBindingInterface: " + bindableItem.HasBindingInterface);
                        print($"  |   |   L {bindableItem} IOControl: " + bindableItem.IOControl);
                        print($"  |   |   L {bindableItem} IsBindingInterface: " + bindableItem.IsBindingInterface);
                        print($"  |   |   L {bindableItem} IsBound: " + bindableItem.IsBound);
                        print($"  |   |   L {bindableItem} Type: " + bindableItem.Type);
                        print($"  |   |   L {bindableItem} Value: " + bindableItem.Value);
                    }
                    print("  |   L BoundItems: " + binding.Expression.BoundItems);
                    var boundItemList = binding.Expression.BoundItems.ToList();
                    foreach (var boundItem in boundItemList) {
                        print($"  |       L {boundItem} AllowedAccess: " + boundItem.AllowedAccess);
                        print($"  |       L {boundItem} BindingInterface: " + boundItem.BindingInterface);
                        print($"  |       L {boundItem} DefaultAccess: " + boundItem.DefaultAccess);
                        print($"  |       L {boundItem} Expression: " + boundItem.Expression);
                        print($"  |       L {boundItem} HasBindingInterface: " + boundItem.HasBindingInterface);
                        print($"  |       L {boundItem} IOControl: " + boundItem.IOControl);
                        print($"  |       L {boundItem} IsBindingInterface: " + boundItem.IsBindingInterface);
                        print($"  |       L {boundItem} IsBound: " + boundItem.IsBound);
                        print($"  |       L {boundItem} Type: " + boundItem.Type);
                        print($"  |       L {boundItem} Value: " + boundItem.Value);
                    }
                    print("  L ForcedValue: " + binding.ForcedValue);
                    print("  L Forced: " + binding.Forced);
                    print("  L Enabled: " + binding.Enabled);
                    print("  L Active: " + binding.Active);
                    print("  L AccessRights: " + binding.AccessRights);
                }
            }
            PrintAllBindingInfo.Value = false;
        }
        #endregion
    }
}