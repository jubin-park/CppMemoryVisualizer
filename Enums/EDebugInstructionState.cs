namespace CppMemoryVisualizer
{
    public enum EDebugInstructionState
    {
        STANDBY,
        INITIALIZING,
        DEAD,

        START_DEBUGGING,

        GO,
        STEP_OVER,
        STEP_IN,
        ADD_BREAK_POINT,
        REMOVE_BREAK_POINT,
    }
}
