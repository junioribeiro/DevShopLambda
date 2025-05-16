using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Options;
using System.Net.Http;
using Upload.S3.API.Application.Domain;

namespace Upload.S3.API.Application.Services
{
    public class AmazonS3Service : IAmazonS3Service
    {
        public string AwsKeyId { get; private set; }
        public string AwsKeySecret { get; private set; }
        public BasicAWSCredentials awsCredentials { get; private set; }

        private readonly IAmazonS3 _amazonS3Client;

        public AmazonS3Service(IOptions<AWSCredentialSetup> credential)
        {
            AwsKeyId = credential.Value.AwsKeyId;
            AwsKeySecret = credential.Value.AwsKeySecret;

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

        public async Task<string> GeneratePresignedURL(string bucket, string key, double duration = 1)
        {
            AWSConfigsS3.UseSignatureVersion4 = true;
            string urlString = string.Empty;
            try
            {
                var request = new GetPreSignedUrlRequest()
                {
                    BucketName = bucket,
                    Key = key,
                    Expires = DateTime.UtcNow.AddHours(duration),
                };
                urlString = await _amazonS3Client.GetPreSignedURLAsync(request);
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"Error:'{ex.Message}'");
            }

            return urlString;
        }

        /// <summary>
        /// Gera um URL presignado que será usado para fazer upload de um objeto para um bucket Amazon S3
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="key"></param>
        /// <param name="duration"></param>
        /// <returns>A URL gerado</returns>
        public async Task<string> GeneratePreSignedURL(string bucket, string key, double duration)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucket,
                Key = key,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddHours(duration),
            };

            string url = await _amazonS3Client.GetPreSignedURLAsync(request);
            return url;
        }

        /// <summary>
        /// Carrega um único arquivo do computador local para um bucket S3.
        /// </summary>       
        /// <param name="bucket">O nome do bucket S3 onde os arquivos serão armazenados.</param>
        /// <param name="fileName">O nome do arquivo a ser carregado.</param>
        /// <param name="localPath">O caminho local onde o arquivo está armazenado.</param>
        /// <returns>Um valor booleano que indica o sucesso da ação.</returns>
        public async Task<bool> UploadSingleFileAsync(string bucket, string fileName, string localPath)
        {
            if (File.Exists($"{localPath}\\{fileName}"))
            {
                try
                {
                    var transferUtil = new TransferUtility(_amazonS3Client);
                    await transferUtil.UploadAsync(new TransferUtilityUploadRequest
                    {
                        BucketName = bucket,
                        Key = fileName,
                        FilePath = $"{localPath}\\{fileName}",
                    });

                    return true;
                }
                catch (AmazonS3Exception s3Ex)
                {
                    Console.WriteLine($"Could not upload {fileName} from {localPath} because:");
                    Console.WriteLine(s3Ex.Message);
                    return false;
                }
            }
            else
            {
                Console.WriteLine($"{fileName} does not exist in {localPath}");
                return false;
            }
        }

        /// <summary>
        /// Carrega todos os arquivos em um diretório local para um diretório em um bucket S3.
        /// </summary>
        /// <param name="bucket">O nome do bucket S3 onde os arquivos serão armazenados.</param>
        /// <param name="keyPrefix">O prefixo da chave é o diretório S3 onde os arquivos serão armazenados.</param>
        /// <param name="localPath">O diretório local que contém os arquivos a serem carregados.</param>
        /// <returns>Um valor booleano indicando os resultados da ação.</returns>
        public async Task<bool> UploadFullDirectoryAsync(string bucket, string keyPrefix, string localPath)
        {
            var transferUtil = new TransferUtility(_amazonS3Client);
            if (Directory.Exists(localPath))
            {
                try
                {
                    await transferUtil.UploadDirectoryAsync(new TransferUtilityUploadDirectoryRequest
                    {
                        BucketName = bucket,
                        KeyPrefix = keyPrefix,
                        Directory = localPath,
                    });

                    return true;
                }
                catch (AmazonS3Exception s3Ex)
                {
                    Console.WriteLine($"Can't upload the contents of {localPath} because:");
                    Console.WriteLine(s3Ex?.Message);
                    return false;
                }
            }
            else
            {
                Console.WriteLine($"The directory {localPath} does not exist.");
                return false;
            }
        }

        /// <summary>
        /// Faça o download de um único arquivo de um balde S3 para o computador local
        /// </summary>
        /// <param name="bucket">O nome do balde S3 contendo o arquivo para download.</param>
        /// <param name="key">O nome do arquivo para baixar.</param>
        /// <param name="localPath">O caminho no computador local onde o arquivo baixado será salvo.</param>
        /// <returns>Um valor booleano indicando os resultados da ação.</returns>
        public async Task<bool> DownloadSingleFileAsync(string bucket, string key, string localPath)
        {
            var transferUtil = new TransferUtility(_amazonS3Client);
            await transferUtil.DownloadAsync(new TransferUtilityDownloadRequest
            {
                BucketName = bucket,
                Key = key,
                FilePath = $"{localPath}\\{key}"
            });

            return (File.Exists($"{localPath}\\{key}"));
        }


        /// <summary>
        /// Baixa o conteúdo de um diretório em um bucket S3 para um diretório no computador local.
        /// </summary>
        /// <param name="bucket">O bucket que contém os arquivos para download.</param>
        /// <param name="s3Path">O diretório S3 onde os arquivos estão localizados.</param>
        /// <param name="localPath">O caminho local onde os arquivos serão salvos. </param>
        /// <returns>Um valor booleano indicando os resultados da ação.</returns>
        public async Task<bool> DownloadS3DirectoryAsync(string bucket, string s3Path, string localPath)
        {
            int fileCount = 0;

            // If the directory doesn't exist, it will be created.
            if (Directory.Exists(s3Path))
            {
                var files = Directory.GetFiles(localPath);
                fileCount = files.Length;
            }

            var transferUtil = new TransferUtility(_amazonS3Client);
            await transferUtil.DownloadDirectoryAsync(new TransferUtilityDownloadDirectoryRequest
            {
                BucketName = bucket,
                LocalDirectory = localPath,
                S3Directory = s3Path,
            });

            if (Directory.Exists(localPath))
            {
                var files = Directory.GetFiles(localPath);
                if (files.Length > fileCount)
                {
                    return true;
                }

                // Nenhuma alteração no número de arquivos. Suponha o download falhou                
                return false;
            }

            // O diretório local não existe. Nenhum arquivo foi baixado.
            return false;
        }







    }
}

