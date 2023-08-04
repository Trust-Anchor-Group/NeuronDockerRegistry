namespace TAG.Networking.DockerRegistry.Errors
{
    /// <summary>
    /// Docker Error Codes
    /// </summary>
    public enum DockerErrorCode
    {
		/// <summary>
		/// This error may be returned when a blob is unknown to the registry in a specified repository. 
        /// This can be returned with a standard get or if a manifest references an unknown layer during upload.
		/// </summary>
        BLOB_UNKNOWN,

		/// <summary>
		/// The blob upload encountered an error and can no longer proceed.
		/// </summary>
		BLOB_UPLOAD_INVALID,

		/// <summary>
		/// If a blob upload has been cancelled or was never started, this error code may be returned.
		/// </summary>
		BLOB_UPLOAD_UNKNOWN,

		/// <summary>
		/// When a blob is uploaded, the registry will check that the content matches the digest provided by the client. 
		/// The error may include a detail structure with the key “digest”, including the invalid digest string. This 
		/// error may also be returned when a manifest includes an invalid layer digest.
		/// </summary>
		DIGEST_INVALID,

		/// <summary>
		/// This error may be returned when a manifest blob is unknown to the registry.
		/// </summary>
		MANIFEST_BLOB_UNKNOWN,  // blob unknown to registry    

		/// <summary>
		/// During upload, manifests undergo several checks ensuring validity. If those checks fail, this error may be 
		/// returned, unless a more specific error is included. The detail will contain information the failed validation.
		/// </summary>
		MANIFEST_INVALID,

		/// <summary>
		/// This error is returned when the manifest, identified by name and tag is unknown to the repository.
		/// </summary>
		MANIFEST_UNKNOWN,

		/// <summary>
		/// During manifest upload, if the manifest fails signature verification, this error will be returned.
		/// </summary>
		MANIFEST_UNVERIFIED,

		/// <summary>
		/// Invalid repository name encountered either during manifest validation or any API operation.
		/// </summary>
		NAME_INVALID,

		/// <summary>
		/// This is returned if the name used during an operation is unknown to the registry.
		/// </summary>
		NAME_UNKNOWN,           // repository name not known to registry   

		/// <summary>
		/// Returned when the “n” parameter (number of results to return) is not an integer, or “n” is negative.
		/// </summary>
		PAGINATION_NUMBER_INVALID,

		/// <summary>
		/// When a layer is uploaded, the provided range is checked against the uploaded chunk. This error is returned if the 
		/// range is out of order.
		/// </summary>
		RANGE_INVALID,          // invalid content range   

		/// <summary>
		/// When a layer is uploaded, the provided size will be checked against the uploaded content. If they do not match, 
		/// this error will be returned.
		/// </summary>
		SIZE_INVALID,           // provided length did not match content length    

		/// <summary>
		/// During a manifest upload, if the tag in the manifest does not match the uri tag, this error will be returned.
		/// </summary>
		TAG_INVALID,            // manifest tag did not match URI  

		/// <summary>
		/// The access controller was unable to authenticate the client. Often this will be accompanied by a Www-Authenticate 
		/// HTTP response header indicating how to authenticate.
		/// </summary>
		UNAUTHORIZED, 

		/// <summary>
		/// The access controller denied access for the operation on a resource.
		/// </summary>
		DENIED,  

		/// <summary>
		/// The operation was unsupported due to a missing implementation or invalid set of parameters.
		/// </summary>
		UNSUPPORTED
	}
}
