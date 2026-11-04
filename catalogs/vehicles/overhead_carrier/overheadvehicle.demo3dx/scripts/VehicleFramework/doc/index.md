## About

The VehicleFramework script is an implementation of a vehicle movement system for Emulate3D. It is coded entirely in C# and is designed to be optimized for simulation. One of the primary uses of the Emulate3D.VehicleFramework package, and the recommended starting point, is the [Emulate3D.Vehicles](/packages/pkg_emulate3d_vehicles) package.

---

## Uses

Provides native objects, and an API for vehicle simulation and emulation in a variety of scenarios. Facilitates track-based movement of vehicles, and also off-track movement for certain vehicle types. Referenced by the [Emulate3D.Vehicles](/packages/pkg_emulate3d_vehicles) package, and others.

---

## Documentation

See [Emulate3D.Vehicles](/packages/pkg_emulate3d_vehicles).

---

## Tutorials

See [Emulate3D.Vehicles](/packages/pkg_emulate3d_vehicles).

---

## Training Videos

See [Emulate3D.Vehicles](/packages/pkg_emulate3d_vehicles).

---

## Revision History

**2.0.1**
- Fix issue where the NumVehiclesEnabled property would not be applied.

**2.0.0**
- Remove built in FlowControl and instead use the Emulate3D.FlowControlFramework package.
- The vehicles widget on vehicle logic now returns both on-track and off-track vehicles, instead of only on-track vehicles.

**1.10.1**
- Fixed stack overflow exception when using strict speed limit mode.

**1.10.0**
- Increase accessibility of CustomLHD methods/properties and make them virtual to support overriding forklift behaviour in C#.
- Add extension methods for IAnimationSegment/IAnimationPath.
- Add ability to override DriveModule/DriveTask methods.
- Add SpeedLimitMode property to vehicles which has the following options:
    Default: The default mode of speed limit enforcement. A track section will apply its speed limit
             to the vehicle once the vehicle's origin enters the track section, and it will not be cleared
             until the vehicle's origin leaves the track section.
    Strict: The vehicle will decelerate to obey the speed limit of track segments ahead of time.
            When the vehicle's front edge reaches the segment it will already be obeying the speed limit
            and when the rear edge of the vehicle clears the segment it will stop obeying the speed limit.
            Vehicles will travel at or below track section speed limits when any part of the vehicle is
            within a speed limit segment on the current path.
    Manual: Speed limits from track sections will not be applied to vehicles. Speed limits may be set
            via scripting without being overridden by internal Vehicle Framework behaviours.
- Fix issue where the simulation would stop when MagneMotion vehicles overlap.
- Fix issue where AMRs would only use one track section in + shaped lifts.
- Improved performance when resetting AMR models.
- Fix issue where an exception would be thrown due to incorrect event scheduling.
- Fix issue where a model would hang while being run.
- Update off track vehicle animation on motor enabled state and animation state flag changes.

**1.9.2**
- Fix issue where approach event would be scheduled early for AGV.

**1.9.1**
- Fix issue where track connection changes via scripting would not cause the track lines to update.

**1.9.0**
- Fix issue where vehicles would not drive to correct level in a multiple level system to pick up/deliver loads.
- Added additional options to DriveOptions used in path finding to improve start/end segment selection in multiple level systems.
- Add TargetIsFinal and Targets to AnimationStateView.
- Fix issue where approach event would be incorrectly triggered twice for a control point.
- Add "cancel divert {vehicle}" widget to control points.

**1.8.2**
- Fix issue where an exception would be thrown when running model with UpdateVehiclesOnPhysicsTimeStep set to true.

**1.8.1**
- Only create zone indicators for MagneMotion motor blocks if a controller is connected to the track.
- Fix issue where track system would not be disposed on reset, and track items would still reference it.
- Fix memory leak caused by not disposing of charge systems.

**1.8.0**
- Support for multiple vehicles sharing a zone on initialize for MagneMotion vehicles.
- Performance improvements.
- Fix infinite loop when using a non-zero value for AccumulationReleaseDelay.

**1.7.0**
- Add API method to wait until a vehicle is stationary.
- Fix issue where no exception was thrown for vehicle with residual speed and no animator.

**1.6.0**
- Add "is looking for work {vehicle}" widget to VehicleLogic.
- Add "release current job {vehicle}" and "can cancel {job}" widgets to VehicleLogic.
- Add "transfer job {obj}", "first load {job}", "first tx {job}", "first rx {job}", and "first assignment {job}" widgets to VehicleLogic.
- Fix issue where Sim3D job statistics would be incorrect when releasing jobs.
- Fix issue where order list could not be retrieved from an element of a VisualList.
- Fix issue where front/rear offsets would be used incorrectly to schedule control point events when a ground-based vehicle reverses direction.
- Fix issue where TotalDistanceTravelled/TotalDurationTravelled vehicle properties would be reset when a vehicle performs an off-track move.
- Add OptimizedAccumulationEnabled boolean property to vehicles.
- Add AccumulationReleaseDelay double property to vehicles.

**1.5.0**
- Fix issue where Dual LHD AGV would pick up two loads to the same LHD.
- Add a variety of custom properties to the ASRS Crane component to improve functionality.
- Performance improvements for MagneMotion models.
- Fix issue where approach events would be incorrectly missed.
- Fix issue where Tri/Quad Tracker Vehicle would not be resized correctly.

**1.4.1**
- Improve memory usage and reduce memory churn for MagneMotion models.
- Make VehicleCount property on ControlPoints read only.
- Update ResourceCapacity property description for zone and gate control points.

**1.4.0**
- Fix issue where interface property values would show as empty strings in the property grid.
- Add DirectionOption to DriveOptions.
- Fix issue where vehicle orientation would be reversed incorrectly when changing segments.
- Add "wait until stopped {vehicle}" widget to ControlPoint and VehicleLogic.
- Add "stop {vehicle} and cancel drive command" to VehicleLogic.
- Fix issue where vehicle would stop due to its front sensor being blocked and not clear for some time.

**1.3.2**
- Fix issue where vehicle would not stop when its motor was disabled.
- Fix very slow script replacement.
- Prevent a StackOverflowException in overhead systems.

**1.3.1**
- Fix AnimationStateView not showing.
- Fix issue where vehicle would not stop when changing direction when moving between perpendicular segments.
- Fix issue where vehicle would drive while inhibited.
- Fix exception when giving drive command to forklift.
- Performance improvements.

**1.3.0**
- Remove misleading Direction, Speed, and State properties from track sections.
- Fix issue where vehicle would not respond to VehicleList property updates.
- Add "wait for job assigned {vehicle}" widget to VehicleLogic.
- Add JobAssigned Action to JobModule.
- Add JobStartedSignal, JobCompletedSignal, JobAssignedSignal to JobModule.
- Fix issue where overhead vehicles would overlap.
- Add per vehicle toggle for pickup job assignment via a CanBeAssigned property on each vehicle.

**1.2.4**
- Fix issue where vehicle failed to maintain accumulation distance.

**1.2.3**
- Fix issue where PickupJobCombiner would not work with left/right connectors.

**1.2.2**
- Fix exception when driving to current location with target offset.
- Improve error message when the Vehicles property is not set on a Vehicle Creator.
- Fix issue where vehicle could not reach destination, and would oscillate on the track.

**1.2.1**
- Fix ZoneBlock component.
- Improve performance of dogs.

**1.2.0**
- Fix issue where shuttle track segment would not operate correctly for certain track configurations.
- Add ability for Dual LHD AGV to pick up and deliver continuously while aligned with the pickup/delivery locations.
- Add additional QuickLogic widgets to support backwards driving forklifts.
- Fix issue where vehicle wouldn't decelerate to a stop when changing direction after diverting.
- Fix issue with Quad Tracker Vehicle reparenting.
- Ensure property grid is refreshed when changing a custom property value adds/removes other properties from view.
- Fix issues with vehicles overlapping on a simple single-drive loop.

**1.1.0**
- Initial release.
