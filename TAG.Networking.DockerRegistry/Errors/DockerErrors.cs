namespace TAG.Networking.DockerRegistry.Errors
{
	/// <summary>
	/// Information about one or more Docker Errors
	/// </summary>
	public class DockerErrors
	{
		/// <summary>
		/// Information about one or more Docker Errors
		/// </summary>
		/// <param name="Errors">Errors</param>
		public DockerErrors(params DockerError[] Errors)
		{
			this.Errors = Errors;
		}

		/// <summary>
		/// Information about one or more Docker Errors
		/// </summary>
		/// <param name="ErrorCode">Error code.</param>
		public DockerErrors(DockerErrorCode ErrorCode)
			: this(new DockerError(ErrorCode))
		{
		}

		/// <summary>
		/// Information about one or more Docker Errors
		/// </summary>
		/// <param name="ErrorCode">Error code.</param>
		/// <param name="Message">Error message.</param>
		public DockerErrors(DockerErrorCode ErrorCode, string Message)
			: this(new DockerError(ErrorCode, Message))
		{
		}

		/// <summary>
		/// Information about one or more Docker Errors
		/// </summary>
		/// <param name="ErrorCode">Error code.</param>
		/// <param name="Message">Error message.</param>
		/// <param name="Details">Context-specific details.</param>
		public DockerErrors(DockerErrorCode ErrorCode, string Message, object Details)
			: this(new DockerError(ErrorCode, Message, Details))
		{
		}

		/// <summary>
		/// Errors
		/// </summary>
		public DockerError[] Errors { get; set; }
	}
}
