using System;
using System.Collections.Generic;
using UnityEngine;


public class PathSmoother {
    private const float CHECK_INTERVAL = 0.25f; // Distance between path checks
    private const float SMOOTH_WEIGHT = 0.5f; // Weight for path smoothing
    private const int MAX_SMOOTH_ITERATIONS = 10; // Maximum smoothing iterations

    public List<Vector3> SmoothPath(List<Vector3> originalPath, Func<float, float, bool> possibleFunc) {
        if (originalPath.Count <= 2) return originalPath;

        var smoothedPath = new List<Vector3>(originalPath);

        for (int iteration = 0; iteration < MAX_SMOOTH_ITERATIONS; iteration++) {
            bool changed = false;
            
            for (int i = 1; i < smoothedPath.Count - 1; i++) {
                Vector3 prev = smoothedPath[i - 1];
                Vector3 current = smoothedPath[i];
                Vector3 next = smoothedPath[i + 1];

                // Calculate smoothed position
                Vector3 smoothed = Vector3.Lerp(
                    current,
                    (prev + next) / 2f,
                    SMOOTH_WEIGHT
                );

                // Check if smoothed path is possible
                if (IsPathPossible(current, smoothed, possibleFunc)) {
                    if (Vector3.Distance(smoothed, current) > 0.01f) {
                        smoothedPath[i] = smoothed;
                        changed = true;
                    }
                }
            }

            if (!changed) break; // Stop if no changes were made
        }

        return smoothedPath;
    }

    private bool IsPathPossible(Vector3 start, Vector3 end, Func<float, float, bool> possibleFunc) {
        float distance = Vector3.Distance(start, end);
        Vector3 direction = (end - start).normalized;
        

        for (float dist = 0; dist <= distance; dist += CHECK_INTERVAL) {
            Vector3 checkPoint = start + direction * dist;
            if (!possibleFunc(checkPoint.x, checkPoint.z)) {
                return false;
            }
        }

        return true;
    }
}