namespace IPTMGrabber.Utils
{
    internal static class HttpClientExtension
    {
        public static async Task<string> ReadOrDownloadAsync(this HttpClient client, string url, string filename, CancellationToken cancellationToken) 
        {
            if (!File.Exists(filename))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filename)!);

                using var downloadRequest = await client.GetAsync(url, cancellationToken);
                downloadRequest.EnsureSuccessStatusCode();

                await File.WriteAllTextAsync(filename, await downloadRequest.Content.ReadAsStringAsync(cancellationToken), cancellationToken);
            }

            return await File.ReadAllTextAsync(filename, cancellationToken);
        }
    }
}
