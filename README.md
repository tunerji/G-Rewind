
---

# G-Rewind

### G-Code Reversal and Optimization Tool

**G-Rewind** is a specialized tool designed to reverse G-code files, reorganize feed rates, and manage Z parameters to simulate reverse 3D printing. It intelligently removes redundant commands, optimizes motion paths, and ensures the integrity of reversed operations. This tool is ideal for reverse motion simulations, G-code optimization, and analyzing 3D printing paths.

## Features

- **G-Code Reversal:** Reverses the order of G-code instructions to simulate reverse motion.
- **Feed Rate Optimization:** Analyzes and reorganizes feed rates for optimal movement.
- **Z Parameter Management:** Adjusts Z coordinates to ensure smooth, consistent layer transitions.
- **Redundant Command Removal:** Cleans up redundant feed rate (`F`) and Z (`Z`) commands.
- **Initialization Block Management:** Handles and removes initialization blocks from G-code generated by slicers like Cura.
- **Safe Z Injection:** Injects a safe Z movement at the beginning of the reversed G-code to avoid collisions.

## Installation

### Prerequisites

- .NET 8.0 or higher
- Basic knowledge of G-code

### Cloning the Repository

To clone the repository, use the following command:

```bash
git clone https://github.com/yourusername/G-Rewind.git
cd G-Rewind
```

### Building the Project

1. Open the solution in Visual Studio.
2. Restore the NuGet packages if needed.
3. Build the solution to generate the executable.

### Running the Tool

Once built, the tool can be executed from the command line or by running the generated executable directly.

## Usage

1. Place your input G-code files in the `g-codes/input` directory.
2. Configure the `config.json` file in the `g-codes/config` directory to match your machine's settings.
3. Run the tool. The processed G-code will be saved in the `g-codes/output` directory.

## G-Code Overview

G-code is the language used to instruct 3D printers and CNC machines. Each line of G-code consists of commands that move the machine's axes, control feed rates, and manage other functions. Below is a brief explanation of common G-code commands:
The G-code used in for tests generated using Cura Slicer.
### Common G-Code Commands

- **G0/G1:** Linear move. G0 is a rapid move, and G1 is a controlled move at a specified feed rate.
- **F:** Feed rate. Specifies the speed at which the print head moves.
- **X, Y, Z:** Coordinates. Defines the position of the print head in the 3D space.
- **M104/M109:** Set extruder temperature.
- **M140/M190:** Set bed temperature.
- **M82/M83:** Set extrusion mode to absolute (M82) or relative (M83).
- **G28:** Home all axes.

### Example G-Code Snippet

```gcode
G1 F1500 X0.00 Y-550 Z1.25  ; Move to coordinates with feed rate of 1500
G0 F4000 X0.00 Y0.00   ; Rapid move to new coordinates
G1 F1200 X476.314 Y275      ; Controlled move to new coordinates
G1 X0.00 Y0.00              ; Move back to start point
```

### Reversed G-Code Example

After processing with G-Rewind, the G-code might look like this:

```gcode
G1 X0.00 Y0.00 Z1.25        ; Move back to start point
G0 F4000 X0.00 Y0.00   ; Rapid move to previous coordinates
G1 F1500 X476.314 Y275 ; Controlled move to previous position
G1 X0.00 Y-550         ; Reverse move to the starting coordinates
```


# G-Rewind

## G-Code Reversal and Optimization Tool

G-Rewind is a specialized tool designed for reversing G-code files, reorganizing feed rates, and optimizing Z parameters to simulate reverse 3D printing. The tool is capable of removing redundant commands, optimizing machine movements, and ensuring the integrity of reversed operations, making it ideal for reverse motion simulations and G-code optimization.

## Directory Structure

The following directory structure is used for the project:

project-root/
│
├── config/
│   └── config.json
│
└── g-codes/
    ├── input/
    ├── output/
    └── resume/

### Explanation:

- **config/**: Contains the `config.json` file where you can configure parameters such as Safe Z offset, User-Defined Top and Bottom Z limits, and other machine-specific settings.
  
- **g-codes/**:
  - **input/**: Place your G-code files here before processing.
  - **output/**: The processed and reversed G-code files will be saved here.
  - **resume/**: This directory can be used for storing intermediary or resumed G-code states if required.

## Process Overview

G-Rewind processes G-code files through the following steps:

1. **Reading the G-Code File**:
   - The tool reads the input G-code file from the `g-codes/input/` directory.

2. **Identifying and Splitting the File**:
   - The G-code is divided into three main blocks: the start block, motion block, and end block.
   - The motion block is where the actual movement commands (G0, G1, etc.) reside.

3. **Trimming Motion Blocks**:
   - The tool trims the motion block to remove any commands outside the user-defined Z limits. This ensures that only the relevant portions of the G-code are processed.

4. **Reversing the Motion Block**:
   - The motion block is reversed to simulate the reverse motion of the original G-code. This is essential for reverse simulations where the motion needs to be played back in the opposite direction.

5. **Tagging**:
   - Each line in the reversed motion block is tagged with appropriate Z and F values which are belongs to the blocks. This ensures that the reversed G-code maintains proper order and structure.

6. **Removing Redundant Tags**:
   - The tool removes redundant F and Z commands to streamline the G-code, ensuring that the machine performs only the necessary movements.

7. **Tweaks and Final Adjustments**:
   - Additional tweaks are applied, such as inserting a safe Z command before the motion block starts. This ensures the machine moves to a safe height before starting the reversed motion.

8. **Saving the Output**:
   - The processed G-code is saved in the `g-codes/output/` directory. The file is optimized for reverse motion simulations and ready for use.

## Configuration

G-Rewind uses a `config.json` file located in the `config/` directory to configure various parameters:

```json
{
  "SafeZOffset": 10.0,
  "UserDefinedTopZ": 200.0,
  "UserDefinedBottomZ": 0.0,
  "MachineMaxZ": 200.0,
  "MachineMinZ": 0.0,
  "MotionCommands": [
    "G0",
    "G1"
  ],
  "CoordinateCommands": [
    "X",
    "Y",
    "Z"
  ],
  "FeedRateCommands": [
    "F"
  ]
}




## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please fork this repository and submit a pull request with your improvements.

## Contact

For any inquiries or issues, please contact.

---

