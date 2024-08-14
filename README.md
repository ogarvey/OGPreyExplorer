# OGPreyExplorer (OGPE)

Intended as an all-in-one tool for the extraction, preview and conversion of Prey (2017) assets.

![image](https://github.com/user-attachments/assets/fff764a2-291b-46fa-87be-f0ea397cac3f)


## Features
### Currently Supported
- PAK File extraction
- Model File conversion (.cgf, .skin) to (dae and gltf)
  
### Planned
- More Model File Formats, input and ouptut
- Model Preview
- Texture Recombiner

## How to use
- Set the paths to the GameSDK folder for Prey, the path to PreyConvert.exe (included in the Assets folder), and the output folder in the appsettings.json file (Noesis isn't currently used, but will be at some point).
- Run the app
- Use the file explorer view at top left to highlight and extract the PAK files to the output folder specified above, the UI will hang while the pak is extracted :(
- Once extracted, you should see the files in the bottom left explorer view, simply highlight the .cgf or .skin file you wish to export the models from
- Once done, you should have both a .dae and .gltf file for the exported model, ready to use in Blender
  
## Thanks
@Markemp for his work on [CGF-Converter](https://github.com/Markemp/Cryengine-Converter)
