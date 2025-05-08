Model created in 18.0.1.39

See [Scripted Scenarios](https://store.sim3d.com/demo3d_2025/test_runner_scripted_scenarios) for more information and [Scripted Scenario Example](https://store.sim3d.com/demo3d_2025/testrunner_scripted_scenario_example) for a step by step example of setting up a Scripted Scenario.

# Simple Example
![Example Image](ExampleImage.png)
There are two Scripted Scenarios in this example, found in `DocumentScript.cs`.

Both tests are testing that `PhotoEye1.BlockedCount` is greater than 20 after 40 seconds but they have different initial parameters.
`Test1` sets the conveyor speed to `0.5m/s` and is expected to fail.
`Test2` sets the conveyor speed to `5m/s` and is expected to pass.
