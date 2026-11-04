---
name: conveyor-line
description: Layout, naming and control rules for the conveyor line in this model. Read this before reading or changing the conveyors, and use the model's GetLineStatus, SetConveyorSpeed and SetConveyorMotor tools instead of the generic property tools.
---

# Conveyor Line

This model is a single straight line of three belt conveyors. Loads travel in the +X direction from `CV_01` to `CV_03`.

## Naming

- Conveyors are named `CV_nn`, numbered in flow order starting at `CV_01`.
- The visual that holds the `ConveyorLineMcp` script is named `McpTools`. Do not move, rename or delete it.

## Tools

Use these custom tools rather than `GetProperties` or `ModifyVisual` for the conveyors:

| Tool | Use it to |
| --- | --- |
| `GetLineStatus` | List every conveyor in flow order with its length, motor state and speed. Call this first. |
| `SetConveyorSpeed` | Set one conveyor's motor speed, from 0 to 2 m/s. |
| `SetConveyorMotor` | Turn one conveyor's motor on or off. |

## Control rules

- Keep each conveyor at least as fast as the one before it, so loads never bunch up at a transfer.
- To change the whole line's speed, set the conveyors from `CV_03` back to `CV_01` when speeding up, and from `CV_01` forward when slowing down. The rule above then holds after every call.
- To stop the line, turn off `CV_01` first and the downstream conveyors after it.
- Report the line status back to the user after every change.

## References

Use `ReadSkillResource` with `References/LineLayout.md` for each conveyor's position, length and default speed.
