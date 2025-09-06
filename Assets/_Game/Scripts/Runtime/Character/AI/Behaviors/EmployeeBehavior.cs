using UnityEngine;
using System.Collections.Generic;

namespace Game.Runtime.Character.AI
{
    public class EmployeeBehavior : BaseAIBehavior
    {
        private EmployeeState currentState = EmployeeState.Idle;
        private Queue<EmployeeTask> taskQueue = new Queue<EmployeeTask>();
        private EmployeeTask currentTask;
        private float idleTimer = 0f;

        public EmployeeBehavior(AICharacterController aiController) : base(aiController) { }

        public override void UpdateBehavior()
        {
            switch (currentState)
            {
                case EmployeeState.Idle:
                    HandleIdle();
                    break;
                case EmployeeState.MovingToTask:
                    HandleMovingToTask();
                    break;
                case EmployeeState.ExecutingTask:
                    HandleExecutingTask();
                    break;
            }
        }

        private void HandleIdle()
        {
            idleTimer += Time.deltaTime;

            // Check for new tasks
            if (taskQueue.Count > 0)
            {
                currentTask = taskQueue.Dequeue();
                currentState = EmployeeState.MovingToTask;
                controller.MoveTo(currentTask.targetPosition);
            }
            else if (idleTimer > 5f) // Generate new task after idle time
            {
                GenerateRandomTask();
                idleTimer = 0f;
            }
        }

        private void HandleMovingToTask()
        {
            if (controller.HasReachedDestination)
            {
                currentState = EmployeeState.ExecutingTask;
                currentTask.startTime = Time.time;
            }
        }

        private void HandleExecutingTask()
        {
            if (Time.time - currentTask.startTime >= currentTask.duration)
            {
                // Task completed
                OnTaskCompleted(currentTask);
                currentTask = null;
                currentState = EmployeeState.Idle;
            }
        }

        public void AddTask(EmployeeTask task)
        {
            taskQueue.Enqueue(task);
        }

        private void GenerateRandomTask()
        {
            // TODO: Get tasks from store management system
            EmployeeTask randomTask = new EmployeeTask
            {
                taskType = (EmployeeTaskType)Random.Range(0, 3),
                targetPosition = GetRandomWorkPoint(),
                duration = Random.Range(3f, 8f)
            };
            
            AddTask(randomTask);
        }

        private Vector3 GetRandomWorkPoint()
        {
            // TODO: Get from store layout
            return Vector3.zero; // Placeholder
        }

        private void OnTaskCompleted(EmployeeTask task)
        {
            Debug.Log($"👷‍♂️ Employee completed task: {task.taskType}");
            // TODO: Notify store management system
        }
    }

    public enum EmployeeState
    {
        Idle,
        MovingToTask,
        ExecutingTask
    }

    public enum EmployeeTaskType
    {
        Restocking,
        Cleaning,
        CustomerService
    }

    [System.Serializable]
    public class EmployeeTask
    {
        public EmployeeTaskType taskType;
        public Vector3 targetPosition;
        public float duration;
        public float startTime;
    }
}