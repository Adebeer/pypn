namespace Pypn.Core
{
    /// <summary>
    /// Provides some control to commands to continue/stop or abort running commands through pipeline stages.
    /// When a command is run on a pipeline, the RunCommand is called on each next/successive pipeline stage.
    /// Once the last pipeline stage is reached, PostRunCommand is called in reverse order from last to first pipeline stage.
    /// </summary>
    public enum PipelineAction
    {
        /// <summary>
        /// (Default). 
        /// For RunCommand: continue running the RunCommand on the next stage. Once the last Stage is reached, call PostRunCommand.
        /// For PostRunCommand: continue running the PostRunCommand on the next (reverse order) pipeline stage
        /// </summary>
        Continue,
        /// <summary>
        /// Terminate immediately and return back to the caller - don't call any more RunCommand/RunPostCommands on any more stages.
        /// </summary>
        Abort,
        /// <summary>
        /// For RunCommand: Stop running RunCommand and start calling PostRunCommand in reverse stage order, starting with the last invoked stage.
        /// For PostRunCommand: continue running the PostRunCommand on the next (reverse order) pipeline stage
        /// </summary>
        Stop
    }
}