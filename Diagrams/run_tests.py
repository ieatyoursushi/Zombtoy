#!/usr/bin/env python3
"""
Simple test runner for diagram generation scripts.
Run this to validate all diagram functionality.
"""
import subprocess
import sys
from pathlib import Path

DIAGRAMS_DIR = Path(__file__).resolve().parent
TEST_FILE = DIAGRAMS_DIR / 'test_diagrams.py'

def run_tests():
    """Run the test suite and return success/failure."""
    print("🧪 Running diagram generation tests...")
    print("=" * 50)
    
    try:
        result = subprocess.run([
            sys.executable, str(TEST_FILE)
        ], capture_output=True, text=True, cwd=DIAGRAMS_DIR)
        
        print(result.stdout)
        
        if result.stderr:
            print("STDERR:")
            print(result.stderr)
        
        if result.returncode == 0:
            print("✅ All tests passed!")
            return True
        else:
            print(f"❌ Tests failed with exit code {result.returncode}")
            return False
            
    except Exception as e:
        print(f"❌ Error running tests: {e}")
        return False

def run_quick_functional_test():
    """Run a quick functional test using actual project files."""
    print("\n🚀 Running quick functional test...")
    print("=" * 50)
    
    try:
        # Run the actual generators
        result = subprocess.run([
            sys.executable, str(DIAGRAMS_DIR / 'generate_all.py')
        ], capture_output=True, text=True, cwd=DIAGRAMS_DIR.parent)
        
        print(result.stdout)
        if result.stderr:
            print("STDERR:")
            print(result.stderr)
        
        # Check if output files exist
        out_dir = DIAGRAMS_DIR / 'out'
        expected_files = [
            'event_flow.puml',
            'class_dependency.puml', 
            'call_graph.puml'
        ]
        
        missing_files = []
        for f in expected_files:
            if not (out_dir / f).exists():
                missing_files.append(f)
        
        if missing_files:
            print(f"❌ Missing output files: {missing_files}")
            return False
        else:
            print("✅ All expected output files generated!")
            
            # Quick content check
            event_puml = (out_dir / 'event_flow.puml').read_text()
            if '@startuml' in event_puml and 'Event' in event_puml:
                print("✅ Event flow diagram contains expected content!")
            else:
                print("⚠️  Event flow diagram may be empty or malformed")
            
            return True
            
    except Exception as e:
        print(f"❌ Error in functional test: {e}")
        return False

if __name__ == '__main__':
    print("🔧 Diagram Generation Test Suite")
    print("=" * 50)
    
    # Run unit tests
    unit_test_success = run_tests()
    
    # Run functional test
    functional_test_success = run_quick_functional_test()
    
    # Summary
    print("\n📊 Test Summary")
    print("=" * 50)
    print(f"Unit Tests: {'✅ PASSED' if unit_test_success else '❌ FAILED'}")
    print(f"Functional Test: {'✅ PASSED' if functional_test_success else '❌ FAILED'}")
    
    if unit_test_success and functional_test_success:
        print("\n🎉 All tests successful! Diagram generation is working correctly.")
        sys.exit(0)
    else:
        print("\n⚠️  Some tests failed. Check output above for details.")
        sys.exit(1)
