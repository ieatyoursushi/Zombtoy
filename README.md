# Zombtoy

**A Unity 3D zombie survival game with multiple perspectives and a .NET backend API**

![Unity Version](https://img.shields.io/badge/Unity-2022.3.37f1-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![Platform](https://img.shields.io/badge/Platform-PC%20%7C%20WebGL-green)

## 📖 Overview

Zombtoy is a zombie survival game built in Unity featuring multiple game modes, weapon systems, and enemy types. Originally created during my high school years as a learning project, it now serves as a comprehensive example of Unity game development with modern architectural patterns and backend integration.

### 🎮 Key / Planned Features

- **Multiple and or Dynamic/Scenic Camera Perspectives**: Switch between isometric and first-person views
- **Diverse Enemy Types**: Fight zom-soldiers, giant/mutant zombtoys, bosses, zomtoys with varying spells / ranged attacks, ect.
- **Weapon Arsenal**: Firearms, rocket launchers, flamethrowers with different ammunition types
- **Health & Stamina System**: Player vitals management (live legacy implementation; refactored component-split version awaiting migration)
- **Backend (current)**: Local and online high score tracking
- **Backend (future)**: full implementation of a multiplayer / CO-OP system wth user-profiles and a basic anticheat.
- **Visual Effects**: Particle systems for combat, explosions, and environmental effects
- **Sound Design**: Comprehensive audio system with SFX and environmental sounds

## 🏗️ Project Structure

```
Zombtoy/
├── Assets/                          # Unity project assets (scenes live at this root: Level1-3, Menu 1-4)
│   ├── Scripts/                     # C# game scripts — see docs/CODE_MAP.md for a per-file guide
│   │   ├── Core/                   # New architecture layer (GameEvents hub, Singleton, state mgmt)
│   │   ├── Player/                 # Player systems (legacy stack active; *Refactored variants dormant)
│   │   ├── Enemy/                  # Enemy health/movement/attack
│   │   ├── Managers/               # Score/Enemy/Item managers (scene-placed singletons)
│   │   ├── UI/                     # Music, menus, score/zombie-count binders
│   │   ├── Weapons/                # New weapon framework (written, not yet wired) + interfaces
│   │   ├── Server/                 # Leaderboard client (+ obsolete Node backend pending removal)
│   │   └── (root .cs files)        # Legacy gameplay: per-weapon scripts, projectiles, camera, items
│   ├── Prefabs/, Guns/, Materials/ # Reusable game objects, weapons, materials
│   └── (enemy/projectile prefabs at Assets/ root: Zombunny, Titan Zombunny, Rocket, …)
├── Backend/
│   ├── ZombtoyBackend/             # .NET 8 Minimal API + SQLite (the real backend — see its README)
│   └── ZombtoyBackend-C/           # Educational C re-implementation (mongoose + SQLite)
├── DevTools/
│   ├── Diagrams/                   # Python static-analysis → PlantUML diagrams & event reports
│   └── shell_scripts/              # open-unity, project-stats, lint helpers
└── docs/                           # Project documentation — start at docs/README.md
    ├── CODE_MAP.md                 # Every script: purpose + active/dormant status
    ├── reexploration/              # 2026 architecture audit (source of truth)
    ├── history/REFACTOR_PLAN.md    # 2025 refactor plan (historical, header corrected)
    ├── backend/DOTNET_BACKEND_INTEGRATION_GUIDE.md  # Target backend design (not implemented)
    └── workflow.md                 # Git + Unity worktree workflow
```

## 🚀 Quick Start

### Unity Client Setup

1. **Prerequisites**
   - Unity 2022.3.37f1 or later
   - Git for cloning the repository

2. **Installation**
   ```bash
   git clone https://github.com/ieatyoursushi/Zombtoy.git
   cd Zombtoy
   ```

3. **Open in Unity**
   - Launch Unity Hub
   - Click "Open" and select the `Zombtoy` folder
   - Let Unity import all assets (may take several minutes)

4. **Run the Game**
   - Open `Assets/Menu.unity` scene
   - Click the Play button in Unity Editor
   - Use the menu to navigate to game levels

### Backend API Setup

The game includes a .NET Minimal API for score tracking and multiplayer preparation.

1. **Prerequisites**
   - .NET 8.0 SDK

2. **Run Backend Locally**
   ```bash
   cd Backend/ZombtoyBackend
   dotnet restore
   dotnet run --urls "http://localhost:3000"
   ```
-so far is just a primitive highscore rest minimal API (will likely keep using .NET minimal for the context of this project).

3. **API Endpoints**
   - `GET /` - Health check
   - `POST /addScore` - Submit score (text/plain or JSON)
   - `GET /getAllScores` - Retrieve all scores

4. **Quick Test**
   ```bash
   # Add a score
   curl -X POST http://localhost:3000/addScore -H 'Content-Type: text/plain' --data '1234'
   
   # Get all scores
   curl http://localhost:3000/getAllScores
   ```
-Websockets coming soon

## 🎯 Game Modes & Scenes

- **Main Menu** (`Menu.unity`) - Primary navigation hub
- **Levels** (`Level1.unity`, `Level2.unity`, `Level3.unity`) - Game scenes representing differing versions; `Level2` is currently **not** in Build Settings (will likely transition to a single-scene full manager-dictated system).
- **Isometric View** - Top-down perspective gameplay
- **First Person** - Immersive FPS experience
- **Settings Menu** - Game configuration
- **Leaderboard** - High score display

## 🏛️ Architecture Overview

### Core Systems

- **GameEvents**: Event-driven architecture for decoupled communication
- **Managers**: Centralized system management (Enemy, Score, GameState, etc.)
- **Player Systems**: Modular player functionality (Health, Movement, Shooting)
- **UI Management**: Scene navigation and user interface handling (most WIP, seamless cross compatibility between legacy code needed)

### Key Components

- **PlayerHealth**: Health and stamina management with event integration (legacy version is what runs; refactored split exists but is not yet wired in)
- **EnemyManager**: Centralized enemy spawning and lifecycle management
- **ScoreManager**: Score tracking and high score persistence
- **WeaponSystem**: New weapon framework (WeaponManager/BaseWeapon/interfaces) — written but **dormant**; gameplay still uses the per-weapon legacy scripts + Inventory
- **ItemManager**: Centralized item spawn-management in the game scene.

> 📌 For the honest per-file breakdown of what is active vs dormant, see [`docs/CODE_MAP.md`](docs/CODE_MAP.md).
  
## 🔧 Development Status

This project is actively being refactored to improve code quality and prepare for multiplayer features.

### Current Focus Areas

- **Event-Driven Architecture**: Migrating from tight coupling to event-based communication
- **Component Separation**: Breaking apart monolithic classes into focused components  
- **Memory Management**: Fixing potential memory leaks in event subscriptions
- **Backend Integration**: Preparing for multiplayer and cloud score synchronization

See [`docs/README.md`](docs/README.md) for the documentation index: [`docs/CODE_MAP.md`](docs/CODE_MAP.md) (what actually runs), [`docs/reexploration/`](docs/reexploration/) (architecture audit + milestones), [`docs/history/REFACTOR_PLAN.md`](docs/history/REFACTOR_PLAN.md) (historical refactor plan), and [`docs/backend/DOTNET_BACKEND_INTEGRATION_GUIDE.md`](docs/backend/DOTNET_BACKEND_INTEGRATION_GUIDE.md) (target backend design).

## 📋 Controls

### Keyboard Controls
- **WASD** - Movement
- **Mouse** - Look around / Aim
- **Left Click** - Shoot
- **R** - Reload
- **Shift** - Sprint
- **Space** - Jump (first-person mode)
- **F** - Flashlight toggle
- **1-9** - Weapon selection
! Inventory and Keying system needs refactoring asap !

### Menu Navigation
- **Mouse** - Navigate menus
- **ESC** - Pause/Menu

## 🎨 Third-Party Assets

This project includes several third-party Unity assets:

- **WarFX**: Combat effects and particle systems
- **Cartoon FX**: Additional visual effects  
- **Sci-Fi Styled Modular Pack**: Environmental models and textures
- **AllSkyFree**: Skybox collection for various environments
--Later conversion to custom made or commercially liscenced assets--
  
## 📈 Performance Considerations

- **Event System**: Static C# events; minimal per-frame allocation
- **Object Pooling**: *Planned* — projectiles/effects currently use Instantiate/Destroy
- **Known hot spots**: remaining `GameObject.Find()` calls (~55) and per-frame polling in legacy `Update()` methods — tracked in the migration issue (#16)

## 🧪 Testing & Debugging

### Debug Tools Available

1. **GameEvents Debug System** - Monitor event flow and subscriptions
2. **Performance Profilers** - Built-in Unity profiling integration
3. **Visual Architecture Diagrams** - See `DevTools/Diagrams/`

### Running Debug Analysis

```bash
cd DevTools/Diagrams
python3 generate_gameevents_debug.py
```

This generates detailed reports about event system health and potential issues.

## 🤝 Contributing

This project welcomes contributions! Areas of focus:

1. **Code Refactoring**: Help finish the legacy→new migration (issue #16; background in `docs/history/REFACTOR_PLAN.md`)
2. **New Features**: Add new enemy types, weapons, or game modes
3. **Performance**: Optimize systems for better performance
4. **Multiplayer/CO-OP**: Implement networking features using the prepared backend
5. **Documentation**: Improve code documentation and guides

### Development Workflow

1. Fork the repository
2. Create a feature branch
3. Follow the existing code patterns and architecture
4. Test your changes thoroughly
5. Submit a pull request with a detailed description

## 📝 Version History

- **Current**: Refactoring phase - Improving architecture and preparing for multiplayer/complete backend system.
- **Original**: High school project - Basic zombie survival gameplay implemented

## 📞 Support

For questions, issues, or contributions:

1. Create an issue on GitHub for bugs or feature requests
2. Check existing documentation in the repository
3. Review the refactoring plans for understanding current development direction

## 🎓 Educational Value

This project serves as an excellent resource for:

- **Unity Game Development**: Complete game project structure
- **C# Programming**: Game programming patterns and practices  
- **Software Architecture**: Event-driven design and refactoring principles
- **Backend Integration**: Unity-to-API communication and the .NET ecosystem specifically in context to backend-development.
- **Performance Optimization**: Unity-specific optimization techniques

## 📄 License

This project is available for educational and development purposes. Third-party assets retain their original licenses.
---

**Built with Unity 2022.3.37f1 • .NET 8.0 • Love for Game Development 🎮**

**Some project images:**
<img width="800" height="497" alt="Screenshot 2025-08-13 at 3 46 55 PM" src="https://github.com/user-attachments/assets/60dc21bf-20c1-4f6e-b901-d4203cd2dd4e" />
<img width="1728" height="1117" alt="Screenshot 2025-08-13 at 3 46 42 PM" src="https://github.com/user-attachments/assets/04979d56-d7bf-45fc-8c13-d9d2155180d4" />
<img width="1279" height="720" alt="Screenshot 2024-11-07 at 9 02 08 PM" src="https://github.com/user-attachments/assets/8ed9b845-a0d7-4542-a9af-3702eeb067f1" />
<img width="1308" height="819" alt="Screenshot 2025-08-26 at 4 10 41 PM" src="https://github.com/user-attachments/assets/d187aa84-6531-4036-b5f8-603fc12001bd" />
<img width="1303" height="816" alt="Screenshot 2025-08-26 at 4 07 57 PM" src="https://github.com/user-attachments/assets/a5e50a0b-87be-48da-8fc4-ee04fafc2c02" />
<img width="1299" height="812" alt="Screenshot 2025-08-26 at 4 07 42 PM" src="https://github.com/user-attachments/assets/c0257546-4686-428d-a5f0-6db3a69c8a61" />
<img width="1303" height="817" alt="Screenshot 2025-08-26 at 4 07 28 PM" src="https://github.com/user-attachments/assets/5bee784a-c730-4fbd-ace6-d94d3323645b" />
<img width="1310" height="821" alt="Screenshot 2025-08-26 at 4 11 51 PM" src="https://github.com/user-attachments/assets/f731f814-2bd6-4003-bfea-ca953758d06e" />
<img width="1916" height="1197" alt="Screenshot 2025-08-28 at 6 50 48 PM" src="https://github.com/user-attachments/assets/099a7c31-1c2f-46b8-9959-f2252984573b" />

