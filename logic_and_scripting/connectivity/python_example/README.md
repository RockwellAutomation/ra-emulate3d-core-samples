# Python Example
|||
|-|-|
|**Emulate3D Version**|18.0.1|
|**Tutorial Link**|N/A|
|||

![Python Example Image](image.png)

## Description
An example model which uses [pythonnet](https://github.com/pythonnet/pythonnet) to execute and retrieve data from Python scripts.

The `PythonExample.OnReset` method initializes the Python environment and executes methods within the `getpyobject.py` Python script. These methods consist of getting a string, updating a string and getting the new value of the string.

## Usage
- Open the Message Log
- Reset the model.

The values retrieved from the Python script are printed to the message log.

