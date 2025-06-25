This Unity-based educational game is designed to teach data visualization literacy, focusing on chart interpretation skills such as line charts, area charts, stacked area charts, and stream graphs.

Prerequisites:

  Unity Editor Version: 60000.1.4f1

  Platform Support:
    Editor Testing: Unity Editor (macOS or Windows)
    Deployment Targets: iOS, WebGL

Getting Started

  Running in Unity Editor
    1. Clone Repository:
      git clone https://github.com/MagdalenaKejstova/VisGame_Interpretation.git
    2. Open Project:
      Launch Unity Hub.
      Add the cloned project folder.
      Ensure you're using Unity Editor version 60000.1.4f1.
    3. Install Dependencies:
      Ensure all packages are imported correctly from the Package Manager (Window > Package Manager).
      Essential packages include:
        TextMeshPro
        Chart And Graph Pro
    4. Open Main Scene:
      Navigate to Assets/Scenes/Menu/MainMenuScene.unity
    5. Run the Game:
      Click the Play button in the Unity Editor to start the game.

Deployment Instructions
  iOS Deployment
    1. Build Settings:
      Set platform to iOS in Build Settings.
    2. Player Settings:
      Configure appropriate identifiers in Player Settings (Bundle ID, Signing Certificates).
    3. Build and Deploy:
      Build project using File > Build Settings > Build.
      Open generated Xcode project and deploy to connected device or simulator.



Project Structure
  Main Directories:
    Assets/Scripts/:
      Contains the core scripts managing game logic.
    Assets/Scenes/:
      Contains scenes for each level and activity.
    Assets/Resources:
      JSON game texts.
    Assets/Resources/_nameOfChart:
      JSON datasets used for populating the charts.
    Assets/UI/:
      Reusable UI and game objects.

  Key Scripts:
    GameManager.cs:
      Handles game states, score management, and global settings.
    LevelManager.cs:
      Controls level transitions and activity management.
    GraphDataFiller.cs:
      Dynamically loads and manages datasets for visualization.
    ActivityManager.cs and derivatives:
      Manages logic specific to each educational activity (AxesActivity, PointPlottingActivity, AreaChartActivity, etc.).

Game Workflow
  Initialization:
    GameManager initializes game state, datasets, and player progress.
  Level Progression:
    LevelManager handles the sequential progression of activities.
  Data Loading:
    GraphDataFiller dynamically loads JSON datasets.
  User Interaction:
    Managed by specific activity scripts inheriting from ActivityManager.
  Scoring and Feedback:
  Scores are tracked and feedback is provided via pop-ups and UI indicators.


