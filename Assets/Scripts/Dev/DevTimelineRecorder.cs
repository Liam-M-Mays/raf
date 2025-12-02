using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Record and replay the last 5 seconds of gameplay.
/// Press R to record/stop, Shift+R to replay.
/// Useful for analyzing complex enemy interactions.
/// </summary>
public class DevTimelineRecorder : MonoBehaviour
{
    public KeyCode recordKey = KeyCode.R;
    public KeyCode replayKey = KeyCode.LeftShift;

    [System.Serializable]
    private class FrameSnapshot
    {
        public Vector3[] enemyPositions;
        public float raftX, raftY;
        public float timestamp;
    }

    private List<FrameSnapshot> recording = new List<FrameSnapshot>();
    private bool isRecording = false;
    private bool isReplaying = false;
    private int replayFrame = 0;
    private const int MAX_FRAMES = 300; // ~5 seconds at 60fps

    void Update()
    {
        if (Input.GetKeyDown(recordKey))
        {
            if (isRecording)
            {
                StopRecording();
            }
            else
            {
                StartRecording();
            }
        }

        if (Input.GetKeyDown(replayKey) && !isRecording && recording.Count > 0)
        {
            StartReplaying();
        }

        if (isRecording)
        {
            RecordFrame();
        }

        if (isReplaying)
        {
            ReplayFrame();
        }
    }

    private void StartRecording()
    {
        isRecording = true;
        recording.Clear();
        Debug.Log("Recording started...");
    }

    private void StopRecording()
    {
        isRecording = false;
        Debug.Log($"Recording stopped. Captured {recording.Count} frames ({recording.Count / 60f:F1}s)");
    }

    private void RecordFrame()
    {
        if (recording.Count >= MAX_FRAMES)
        {
            recording.RemoveAt(0); // Remove oldest frame
        }

        var enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        var positions = new Vector3[enemies.Length];
        for (int i = 0; i < enemies.Length; i++)
        {
            positions[i] = enemies[i].transform.position;
        }

        var raft = GameServices.GetRaft();
        var snapshot = new FrameSnapshot
        {
            enemyPositions = positions,
            raftX = raft != null ? raft.position.x : 0,
            raftY = raft != null ? raft.position.y : 0,
            timestamp = Time.time
        };

        recording.Add(snapshot);
    }

    private void StartReplaying()
    {
        isReplaying = true;
        replayFrame = 0;
        Time.timeScale = 0.5f; // Slow replay
        Debug.Log("Replaying...");
    }

    private void ReplayFrame()
    {
        if (replayFrame >= recording.Count)
        {
            isReplaying = false;
            Time.timeScale = 1f;
            Debug.Log("Replay finished");
            return;
        }

        var frame = recording[replayFrame];
        var enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        
        for (int i = 0; i < Mathf.Min(enemies.Length, frame.enemyPositions.Length); i++)
        {
            enemies[i].transform.position = frame.enemyPositions[i];
        }

        replayFrame++;
    }
}
