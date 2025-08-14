#!/usr/bin/env python3
"""
Test suite for diagram generation scripts.
Tests core functionality without requiring PlantUML installation.
"""
import unittest
from pathlib import Path
import tempfile
import shutil
from unittest.mock import patch, MagicMock
import sys

# Add the Diagrams directory to Python path for imports
DIAGRAMS_DIR = Path(__file__).resolve().parent
sys.path.insert(0, str(DIAGRAMS_DIR))

from common import strip_code, read_cs_files, load_and_strip
from generate_event_flow import collect as collect_events, emit_puml as emit_event_puml
from generate_class_dependency import parse as parse_classes, emit_puml as emit_class_puml
from generate_call_graph import collect as collect_calls, emit_puml as emit_call_puml


class TestCommonUtilities(unittest.TestCase):
    """Test shared utility functions."""
    
    def test_strip_code_removes_comments(self):
        """Test that single and multi-line comments are stripped."""
        code = '''
        class Test {
            // Single line comment
            public void Method() {
                /* Multi-line
                   comment */
                DoSomething();
            }
        }
        '''
        stripped = strip_code(code)
        self.assertNotIn('Single line comment', stripped)
        self.assertNotIn('Multi-line', stripped)
        self.assertIn('class Test', stripped)
        self.assertIn('DoSomething', stripped)
    
    def test_strip_code_removes_string_literals(self):
        """Test that string literals are replaced."""
        code = 'Debug.Log("This is a test message");'
        stripped = strip_code(code)
        self.assertNotIn('This is a test message', stripped)
        self.assertIn('Debug.Log("")', stripped)
    
    def test_read_cs_files_filters_correctly(self):
        """Test that only .cs files are found."""
        with tempfile.TemporaryDirectory() as temp_dir:
            temp_path = Path(temp_dir)
            # Create test files
            (temp_path / 'Test.cs').touch()
            (temp_path / 'Test.txt').touch()
            (temp_path / 'subfolder').mkdir()
            (temp_path / 'subfolder' / 'Another.cs').touch()
            
            cs_files = read_cs_files(temp_path)
            cs_names = [f.name for f in cs_files]
            
            self.assertIn('Test.cs', cs_names)
            self.assertIn('Another.cs', cs_names)
            self.assertNotIn('Test.txt', cs_names)
            self.assertEqual(len(cs_files), 2)


class TestEventFlowGeneration(unittest.TestCase):
    """Test event flow diagram generation."""
    
    def setUp(self):
        """Create a mock C# file structure for testing."""
        self.test_code = {
            'GameEvents.cs': '''
            public static class GameEvents {
                public static event Action OnPlayerDeath;
                public static event Action<int> OnScoreChanged;
                public static event Action OnGameOver;
            }
            ''',
            'ScoreManager.cs': '''
            public class ScoreManager {
                void AddScore(int points) {
                    GameEvents.OnScoreChanged?.Invoke(points);
                }
                void OnGameOver() {
                    GameEvents.OnGameOver?.Invoke();
                }
            }
            ''',
            'UIManager.cs': '''
            public class UIManager {
                void Start() {
                    GameEvents.OnScoreChanged += UpdateScoreText;
                    GameEvents.OnPlayerDeath += ShowGameOver;
                }
                void UpdateScoreText(int score) { }
                void ShowGameOver() { }
            }
            '''
        }
    
    @patch('generate_event_flow.read_cs_files')
    @patch('generate_event_flow.load_and_strip')
    def test_event_collection(self, mock_load, mock_read):
        """Test that events are correctly identified and categorized."""
        # Mock file reading
        mock_files = [Path(name) for name in self.test_code.keys()]
        mock_read.return_value = mock_files
        mock_load.side_effect = lambda p: self.test_code[p.name]
        
        decls, producers, consumers = collect_events()
        
        # Check event declarations
        self.assertIn('OnPlayerDeath', decls)
        self.assertIn('OnScoreChanged', decls)
        self.assertIn('OnGameOver', decls)
        
        # Check producers
        self.assertIn('ScoreManager', producers['OnScoreChanged'])
        self.assertIn('ScoreManager', producers['OnGameOver'])
        
        # Check consumers
        self.assertIn('UIManager', consumers['OnScoreChanged'])
        self.assertIn('UIManager', consumers['OnPlayerDeath'])
    
    def test_event_puml_generation(self):
        """Test PlantUML generation for events."""
        decls = {'OnPlayerDeath', 'OnScoreChanged'}
        producers = {'OnPlayerDeath': {'PlayerHealth'}, 'OnScoreChanged': {'ScoreManager'}}
        consumers = {'OnPlayerDeath': {'UIManager'}, 'OnScoreChanged': {'ScoreDisplay'}}
        
        puml = emit_event_puml(decls, producers, consumers)
        
        self.assertIn('@startuml', puml)
        self.assertIn('@enduml', puml)
        self.assertIn('OnPlayerDeath << (E,#FFCC00) Event >>', puml)
        self.assertIn('PlayerHealth --> OnPlayerDeath : raises', puml)
        self.assertIn('OnPlayerDeath --> UIManager : notifies', puml)


class TestClassDependencyGeneration(unittest.TestCase):
    """Test class dependency diagram generation."""
    
    def setUp(self):
        """Create mock C# files with inheritance."""
        self.test_code = {
            'BaseWeapon.cs': '''
            public interface IWeapon {
                void Fire();
            }
            public abstract class BaseWeapon : MonoBehaviour, IWeapon {
                public abstract void Fire();
            }
            ''',
            'Pistol.cs': '''
            public class Pistol : BaseWeapon {
                public override void Fire() { }
            }
            ''',
            'ScoreManager.cs': '''
            public class ScoreManager : Singleton<ScoreManager> {
                public int Score { get; set; }
            }
            '''
        }
    
    @patch('generate_class_dependency.read_cs_files')
    @patch('generate_class_dependency.load_and_strip')
    def test_class_parsing(self, mock_load, mock_read):
        """Test that class relationships are correctly parsed."""
        mock_files = [Path(name) for name in self.test_code.keys()]
        mock_read.return_value = mock_files
        mock_load.side_effect = lambda p: self.test_code[p.name]
        
        classes, interfaces, extends, implements = parse_classes()
        
        # Check interface detection
        self.assertIn('IWeapon', interfaces)
        
        # Check class detection
        self.assertIn('BaseWeapon', classes)
        self.assertIn('Pistol', classes)
        self.assertIn('ScoreManager', classes)
        
        # Check inheritance
        self.assertIn('MonoBehaviour', extends['BaseWeapon'])
        self.assertIn('BaseWeapon', extends['Pistol'])
        
        # Check interface implementation
        self.assertIn('IWeapon', implements['BaseWeapon'])
    
    def test_generic_base_class_handling(self):
        """Test that generic base classes are handled correctly in PlantUML output."""
        classes = {'ScoreManager': Path('test')}
        interfaces = set()
        extends = {'ScoreManager': {'Singleton<ScoreManager>'}}
        implements = {}
        
        puml = emit_class_puml(classes, interfaces, extends, implements)
        
        # Should strip generic part for PlantUML compatibility
        self.assertIn('Singleton <|-- ScoreManager', puml)
        self.assertNotIn('Singleton<ScoreManager>', puml)


class TestCallGraphGeneration(unittest.TestCase):
    """Test call graph generation."""
    
    def setUp(self):
        """Create mock C# files with method calls."""
        self.test_code = {
            'PlayerController.cs': '''
            public class PlayerController {
                void Update() {
                    InputManager.GetInput();
                    WeaponManager.HandleShooting();
                }
            }
            ''',
            'WeaponManager.cs': '''
            public class WeaponManager {
                void HandleShooting() {
                    ScoreManager.AddPoints(10);
                }
            }
            ''',
            'ScoreManager.cs': '''
            public class ScoreManager {
                void AddPoints(int points) { }
            }
            '''
        }
    
    @patch('generate_call_graph.read_cs_files')
    @patch('generate_call_graph.load_and_strip')  
    def test_call_collection(self, mock_load, mock_read):
        """Test that class-to-class calls are detected."""
        mock_files = [Path(name) for name in self.test_code.keys()]
        mock_read.return_value = mock_files
        mock_load.side_effect = lambda p: self.test_code[p.name.replace('.cs', '.cs')]
        
        classes, edges = collect_calls()
        
        # Check that classes are found
        self.assertIn('PlayerController', classes)
        self.assertIn('WeaponManager', classes)
        self.assertIn('ScoreManager', classes)
        
        # Check call relationships (may vary based on regex matching)
        # At minimum, should detect some calls between classes
        self.assertGreater(len(edges), 0)


class TestIntegration(unittest.TestCase):
    """Integration tests for the full workflow."""
    
    def test_generate_all_workflow(self):
        """Test that the main workflow doesn't crash."""
        with tempfile.TemporaryDirectory() as temp_dir:
            out_dir = Path(temp_dir) / 'out'
            out_dir.mkdir()
            
            # Mock the file system to avoid dependency on actual project structure
            with patch('generate_event_flow.OUT_DIR', out_dir):
                with patch('generate_class_dependency.OUT_DIR', out_dir):
                    with patch('generate_call_graph.OUT_DIR', out_dir):
                        with patch('generate_event_flow.read_cs_files', return_value=[]):
                            with patch('generate_class_dependency.read_cs_files', return_value=[]):
                                with patch('generate_call_graph.read_cs_files', return_value=[]):
                                    # Import and run the generators
                                    import generate_event_flow
                                    import generate_class_dependency  
                                    import generate_call_graph
                                    
                                    # Should not crash even with empty input
                                    generate_event_flow.main()
                                    generate_class_dependency.main()
                                    generate_call_graph.main()
                                    
                                    # Check output files were created
                                    self.assertTrue((out_dir / 'event_flow.puml').exists())
                                    self.assertTrue((out_dir / 'class_dependency.puml').exists())
                                    self.assertTrue((out_dir / 'call_graph.puml').exists())


if __name__ == '__main__':
    # Run the test suite
    unittest.main(verbosity=2)
