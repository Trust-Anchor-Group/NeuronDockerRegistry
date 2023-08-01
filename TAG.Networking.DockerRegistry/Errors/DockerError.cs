namespace TAG.Networking.DockerRegistry.Errors
{
	/// <summary>
	/// Information about a specific Docker Error
	/// </summary>
	public class DockerError
	{
		/// <summary>
		/// Information about a specific Docker Error
		/// </summary>
		/// <param name="ErrorCode">Error code.</param>
		public DockerError(DockerErrorCode ErrorCode)
			: this(ErrorCode, ErrorCode.ToString(), null)
		{
		}

		/// <summary>
		/// Information about a specific Docker Error
		/// </summary>
		/// <param name="ErrorCode">Error code.</param>
		/// <param name="Message">Error Message.</param>
		public DockerError(DockerErrorCode ErrorCode, string Message)
			: this(ErrorCode, Message, null)
		{
		}

		/// <summary>
		/// Information about a specific Docker Error
		/// </summary>
		/// <param name="ErrorCode">Error code.</param>
		/// <param name="Message">Error Message.</param>
		/// <param name="Detail">Context-specific details.</param>
		public DockerError(DockerErrorCode ErrorCode, string Message, object Detail)
		{
			this.Code = ErrorCode;
			this.Message = Message;
			this.Detail = Detail;
		}

		/// <summary>
		/// Error code
		/// </summary>
		public DockerErrorCode Code { get; set; }

		/// <summary>
		/// Human-readable error message.
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// Details (context specific).
		/// </summary>
		public object Detail { get; set; }
	}
}
