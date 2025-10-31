# Changing and Accessing IO Browser Values

|||
|-|-|
|**Emulate3D Version**|18.03.00|
|**Tutorial Link**|N/A|
|||

![](ChangeIOBrowserValuesInScriptExample.gif)

## Description
This example showcases how to force IO Browser server values and how to access all other IO Browser information via script.

Provided is an [example model](ChangingIOBrowserValuesInScriptExample.demo3dx\Model.demo3dx) containing visuals that showcase the [ChangeIOBrowserValues](ChangingIOBrowserValuesInScriptExample.demo3dx\scripts\ChangeIOBrowserValues.cs) script capabilities.

## Usage | Forcing a specific server value example
1. Select the **BoxTube1** visual in the scene.
2. Change the custom property **I_TestBool_ForceValue** to force the IO Browser **Program:ExampleProgram.I_TestBool** server and model value.
3. Oberve the IO Browser values change.

## Usage | Unforcing all server values
1. Select the **BoxTube1** visual in the scene.
2. Change the custom property **UnForceAllBindings** to unforce all IO Browser items.

## Usage | Printing all IO browser information
1. Select the **BoxTube1** visual in the scene.
2. Change the custom property **PrintAllBindingInfo** to print each IO Browser row's info to the **Message Log**.

## Usage | Printing all Binding information
1. Select the **BoxTube1** visual in the scene.
2. Change the custom property **PrintAllBindingInfo** to print the all model **Bindings** (and sub-properties) to the **Message Log**.


