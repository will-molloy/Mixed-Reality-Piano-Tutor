# Mixed Reality Piano Tutor
A Gamified Piano Practice Environment.

*SOFTENG 700AB: Honours Research Project (University of Auckland).*

## Video

[https://www.youtube.com/watch?v=nWIq1QS27Sk](https://www.youtube.com/watch?v=nWIq1QS27Sk)

## Requirements
- HTC Vive/[HTC Vive Pro](https://www.vive.com/nz/product/vive-pro/) (or Oculus Rift, with prefab and setting changes)
  - We used Vive Pro  
- HTC Vive Trackers (controllers work fine)
- HTC Vive Lighthouses
- [ZED Mini stereo camera](https://www.stereolabs.com/zed-mini/)
  - Including [ZED SDK](https://www.stereolabs.com/developers/) (we used 2.4)
  - And matching [Unity plugin](https://github.com/stereolabs/zed-unity/releases)
- Capable computer
- Unity3D
  - We used [Unity 2017.4.8f1 (64-bit) (LTS)](https://unity3d.com/unity/qa/lts-releases)
- MIDI Keyboard
    - We used [Casio CTK-3500](https://www.rockshop.co.nz/gear/Casio-Ctk--3500)

## Usage

### Install
- Clone the repository
- Install Unity, ZED SDK, and SteamVR
- Open the Unity project
- Ensure Vive, ZED, MIDI keyboard etc. are connected to your PC (USB 3.0 and display port)

### Calibrate
- Play the 'PlayMode' scene
- Attach the Vive controller to your keyboard such that it doesn't move
    - Note it must be the correct controller
    - You can change this by setting the 'Left Marker' in the PianoBuilderMarkerHook script (under the 'Piano / Sequencer / Calibration' object) with the other controller found in the 'CameraRig' object
      - Using one marker takes the first markers position ('Left marker')
      - Using two markers takes the centre of the two
- Start the scene, make sure your keyboard controls the scene by clicking on the game screen
- Ensure you can view the virtual piano at the tip of your tracker
    -   If not press 'r' to reset the position
    -   Otherwise your tracker isn't being tracked correctly
- Move the virtual piano with: w, a, s, d
- Rotate the virtual piano with: the arrow keys, z, x
- Scale the virtual piano (you shouldn't need to) with: +, -
- Once the virtual piano overlays with your keyboard save the calibration with: BackSpace

### Play
- Play the 'MainUI' scene
- Navigate the UI to play a MIDI file, set the game speed, and username
- Make sure your keyboard controls the scene by clicking on the game screen
- Press 'Enter' to load the piano roll once in the 'PlayMode' scene
- Press 'h' to toggle the virtual piano
- Press 'o' to toggle the ZED-Mini's occlusion
- Press 'Escape' to quit the song early and view the end-of-session feedback
    - Scroll the feedback with: PageUp, PageDown
    - This will also save the session, hence you can alternatively view this mode from the 'HistoryUI' scene
-  Note you can continue to adjust the calibration while in game
  

### Load MIDI files
- Place your files in 'Assets/MIDI'
    -  They will automatically be detected
- Update MIDI file difficulty associations in the 'Assets/Resources/midi-difficulties.json' file

### Changing defaults
- In the RunTimeSettings.cs class (found in Assets/Scripts) you can edit defaults such as the MIDI directory, and default song chosen for the PlayMode scene
- Future work includes creating a user interface for this

## Contribute
- Feel free to fork the repository
  - You may consider using [git-lfs](https://git-lfs.github.com/)
- Feel free to contact us {wmol664, qhua948}@aucklanduni.ac.nz

