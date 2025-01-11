using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using DocumentFormat.OpenXml.Packaging;

namespace ShapeCrawler.Presentations;

/// <summary>
///     Tracks all media parts to enable de-duplication.
/// </summary>
/// <remarks>
///     Currently only tracks image parts, but can be extended in the future to all media types.
/// </remarks>
internal class MediaCollection
{
    /// <summary>
    ///     Reference to every known image part by the hash of its data stream.
    /// </summary>
    private readonly Dictionary<string, ImagePart> imagePartByHash = [];

    /// <summary>
    ///     Gets the image part associated with the specified file contents hash.
    /// </summary>
    /// <param name="hash">Hash generated by `ComputeFileHash()`.</param>
    /// <param name="part">ImagePart corresponding to the given hash, if found.</param>
    /// <returns>True if a part was found for the specified hash.</returns>
    public bool TryGetImagePart(string hash, out ImagePart part) => this.imagePartByHash.TryGetValue(hash, out part!);


    /// <summary>
    ///     Sets the image part for a given file contents hash.
    /// </summary>
    /// <param name="hash">Hash generated by `ComputeFileHash()`.</param>
    /// <param name="part">ImagePart corresponding to the given hash.</param>
    public void SetImagePart(string hash, ImagePart part) => this.imagePartByHash[hash] = part;
}
