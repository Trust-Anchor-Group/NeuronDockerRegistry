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
		/// Errors
		/// </summary>
		public DockerError[] Errors { get; set; }
	}
}
