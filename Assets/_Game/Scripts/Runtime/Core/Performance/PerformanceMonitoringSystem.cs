using UnityEngine;
using System.Collections.Generic;
using Unity.Profiling;

namespace Game.Runtime.Core.Performance
{
    public class PerformanceMonitoringSystem : MonoBehaviour
    {
        [Header("Monitoring Settings")]
        [SerializeField] private bool enableMonitoring = true;
        [SerializeField] private float updateInterval = 1f;
        [SerializeField] private bool logWarnings = true;
        [SerializeField] private bool showOnScreenDisplay = false;

        [Header("Performance Thresholds")]
        [SerializeField] private float targetFrameRate = 60f;
        [SerializeField] private int maxMemoryMB = 512;
        [SerializeField] private int maxDrawCalls = 500;

        // Performance metrics
        private float _frameRate;
        private float _deltaTime;
        private long _memoryUsage;
        private int _drawCalls;
        private int _triangles;

        // Profiler markers for custom profiling
        private static readonly ProfilerMarker s_CharacterUpdateMarker = new ProfilerMarker("Character.Update");
        private static readonly ProfilerMarker s_InteractionMarker = new ProfilerMarker("Interaction.Process");
        private static readonly ProfilerMarker s_ProductionMarker = new ProfilerMarker("Production.Update");
        private static readonly ProfilerMarker s_AIBehaviorMarker = new ProfilerMarker("AI.Behavior");

        // Performance tracking
        private readonly Dictionary<string, float> _performanceMetrics = new Dictionary<string, float>();
        private float _lastUpdateTime;

        // Memory allocation tracking
        private readonly List<float> _frameTimeHistory = new List<float>(60);
        private readonly List<long> _memoryHistory = new List<long>(60);

        void Start()
        {
            if (enableMonitoring)
            {
                InitializeMonitoring();
            }
        }

        void Update()
        {
            if (!enableMonitoring) return;

            if (Time.time - _lastUpdateTime >= updateInterval)
            {
                UpdatePerformanceMetrics();
                CheckPerformanceThresholds();
                _lastUpdateTime = Time.time;
            }
        }

        private void InitializeMonitoring()
        {
            // Set target frame rate for consistent performance measurement
            Application.targetFrameRate = (int)targetFrameRate;
            
            // Initialize performance tracking
            _lastUpdateTime = Time.time;
            
            Debug.Log("Performance monitoring system initialized");
        }

        private void UpdatePerformanceMetrics()
        {
            // Frame rate calculation
            _deltaTime = Time.unscaledDeltaTime;
            _frameRate = 1f / _deltaTime;

            // Memory usage
            _memoryUsage = System.GC.GetTotalMemory(false) / (1024 * 1024); // Convert to MB

            // Basic rendering stats (simplified since DebugUI.instance is not always available)
            _drawCalls = 0; // Unity's rendering stats are not easily accessible in runtime
            _triangles = 0; // These would need to be tracked manually or via profiler

            // Store history for trend analysis
            StorePerformanceHistory();

            // Update performance metrics dictionary
            _performanceMetrics["FrameRate"] = _frameRate;
            _performanceMetrics["MemoryMB"] = _memoryUsage;
            _performanceMetrics["DrawCalls"] = _drawCalls;
            _performanceMetrics["Triangles"] = _triangles;
        }

        private void StorePerformanceHistory()
        {
            // Keep rolling window of performance data
            _frameTimeHistory.Add(_deltaTime);
            if (_frameTimeHistory.Count > 60)
                _frameTimeHistory.RemoveAt(0);

            _memoryHistory.Add(_memoryUsage);
            if (_memoryHistory.Count > 60)
                _memoryHistory.RemoveAt(0);
        }

        private void CheckPerformanceThresholds()
        {
            if (!logWarnings) return;

            // Check frame rate
            if (_frameRate < targetFrameRate * 0.8f)
            {
                Debug.LogWarning($"Performance Warning: Frame rate dropped to {_frameRate:F1} FPS (target: {targetFrameRate})");
            }

            // Check memory usage
            if (_memoryUsage > maxMemoryMB)
            {
                Debug.LogWarning($"Performance Warning: Memory usage is {_memoryUsage} MB (limit: {maxMemoryMB} MB)");
            }
        }

        void OnGUI()
        {
            if (!enableMonitoring || !showOnScreenDisplay) return;

            // Simple on-screen performance display
            var rect = new Rect(10, 10, 300, 120);
            GUI.Box(rect, "Performance Monitor");

            var labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 12 };
            
            GUI.Label(new Rect(15, 30, 290, 20), $"FPS: {_frameRate:F1}", labelStyle);
            GUI.Label(new Rect(15, 50, 290, 20), $"Memory: {_memoryUsage} MB", labelStyle);
            GUI.Label(new Rect(15, 70, 290, 20), $"Draw Calls: {_drawCalls}", labelStyle);
            GUI.Label(new Rect(15, 90, 290, 20), $"Triangles: {_triangles}", labelStyle);
        }

        // Public static methods for profiling critical sections
        public static ProfilerMarker.AutoScope ProfileCharacterUpdate()
        {
            return s_CharacterUpdateMarker.Auto();
        }

        public static ProfilerMarker.AutoScope ProfileInteraction()
        {
            return s_InteractionMarker.Auto();
        }

        public static ProfilerMarker.AutoScope ProfileProduction()
        {
            return s_ProductionMarker.Auto();
        }

        public static ProfilerMarker.AutoScope ProfileAIBehavior()
        {
            return s_AIBehaviorMarker.Auto();
        }

        // Public API for external performance tracking
        public void RecordCustomMetric(string name, float value)
        {
            _performanceMetrics[name] = value;
        }

        public float GetMetric(string name)
        {
            return _performanceMetrics.TryGetValue(name, out float value) ? value : 0f;
        }

        public Dictionary<string, float> GetAllMetrics()
        {
            return new Dictionary<string, float>(_performanceMetrics);
        }

        // Garbage collection monitoring
        [ContextMenu("Force GC and Log")]
        public void ForceGCAndLog()
        {
            var beforeMemory = System.GC.GetTotalMemory(false);
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            var afterMemory = System.GC.GetTotalMemory(false);
            
            Debug.Log($"GC Collection: Released {(beforeMemory - afterMemory) / (1024 * 1024):F2} MB");
        }

        // Performance report generation
        [ContextMenu("Generate Performance Report")]
        public void GeneratePerformanceReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== Performance Report ===");
            report.AppendLine($"Current FPS: {_frameRate:F1}");
            report.AppendLine($"Average Frame Time: {GetAverageFrameTime():F3}ms");
            report.AppendLine($"Memory Usage: {_memoryUsage} MB");
            report.AppendLine($"Draw Calls: {_drawCalls}");
            report.AppendLine($"Triangles: {_triangles}");
            
            if (_frameTimeHistory.Count > 10)
            {
                report.AppendLine($"Frame Time Variance: {GetFrameTimeVariance():F3}ms");
            }
            
            Debug.Log(report.ToString());
        }

        private float GetAverageFrameTime()
        {
            if (_frameTimeHistory.Count == 0) return 0f;
            
            float sum = 0f;
            foreach (var frameTime in _frameTimeHistory)
            {
                sum += frameTime;
            }
            return (sum / _frameTimeHistory.Count) * 1000f; // Convert to milliseconds
        }

        private float GetFrameTimeVariance()
        {
            if (_frameTimeHistory.Count < 2) return 0f;
            
            float average = GetAverageFrameTime() / 1000f; // Convert back to seconds
            float variance = 0f;
            
            foreach (var frameTime in _frameTimeHistory)
            {
                float diff = frameTime - average;
                variance += diff * diff;
            }
            
            return Mathf.Sqrt(variance / _frameTimeHistory.Count) * 1000f; // Convert to milliseconds
        }
    }

    // Performance profiling attributes for easy integration
    public static class PerformanceProfiler
    {
        public static void BeginSample(string sampleName)
        {
            UnityEngine.Profiling.Profiler.BeginSample(sampleName);
        }

        public static void EndSample()
        {
            UnityEngine.Profiling.Profiler.EndSample();
        }

        // RAII-style profiling scope
        public struct ProfileScope : System.IDisposable
        {
            public ProfileScope(string sampleName)
            {
                UnityEngine.Profiling.Profiler.BeginSample(sampleName);
            }

            public void Dispose()
            {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        public static ProfileScope CreateScope(string sampleName)
        {
            return new ProfileScope(sampleName);
        }
    }
}