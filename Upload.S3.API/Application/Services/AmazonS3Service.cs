using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace Upload.S3.API.Application.Services
{
    public class AmazonS3Service
    {
        public string AwsKeyId { get; private set; }
        public string AwsKeySecret { get; private set; }
        public BasicAWSCredentials awsCredentials { get; private set; }

        private readonly IAmazonS3 _amazonS3Client;

        public AmazonS3Service()
        {
            AwsKeyId = "";
            AwsKeySecret = "";
            awsCredentials = new BasicAWSCredentials(AwsKeyId, AwsKeySecret);
            var config = new AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.USEast1
            };

            _amazonS3Client = new AmazonS3Client(awsCredentials, config);
        }

        public async Task<bool> UploadFileAsync(string bucket, string key, IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var fileTransferUtility = new TransferUtility(_amazonS3Client);

            await fileTransferUtility.UploadAsync(new TransferUtilityUploadRequest
            {
                InputStream = memoryStream,
                Key = key,
                BucketName = bucket,
                ContentType = file.ContentType
            });

            return true;

        }

    }
}
