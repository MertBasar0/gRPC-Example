using Grpc.Core;
using ImageMagick;
using System.Net;
using static gRPCExample.ExampleRPCServices;

namespace gRPCExample.Services
{
    public class ExampleService : ExampleRPCServicesBase
    {
        public string mockPhoto { get; set; } = string.Empty;
        public List<string> mockPhotoList { get; set; }

        public ExampleService()
        { 
            mockPhotoList = new();
        }

        static byte[] DownloadImage(string url)
        {
            try
            {
                using (WebClient client = new())
                {
                    var image = client.DownloadData(new Uri(url));
                    if(image != null)
                    {
                        return image;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

        }

        public override async Task BidirectionalStreamingMethod(IAsyncStreamReader<bidirectionalWayExampleRequest> requestStream, IServerStreamWriter<bidirectionalWayExampleResponse> responseStream, ServerCallContext context)
        {
            try
            {
                List<bidirectionalWayExampleResponse> animalImage = new();
                await foreach (var item in requestStream.ReadAllAsync())
                {
                    byte[] resimByte = DownloadImage(item.ClientSideImage);
                    using (MemoryStream memoryStream = new(resimByte))
                    {
                        using (MagickImage magic = new(memoryStream))
                        {
                            magic.Blur(10, 20);
                            animalImage.Add(new bidirectionalWayExampleResponse()
                            {
                                ManipulatedServerSideImage = magic.ToBase64()+ "\n\n\n\n\n\n\n\n\n\n\n\n\n"
                            });
                        }
                    }
                    Console.WriteLine(item.ClientSideImage);
                }

                foreach (var item in animalImage)
                {
                    await responseStream.WriteAsync(item);
                }
            }
            catch (Exception)
            {

                throw;
            }
           
        }

        public override async Task<exampleResponse> ClientStreamMethod(IAsyncStreamReader<exampleRequest> requestStream, ServerCallContext context)
        {
            await foreach (exampleRequest request in requestStream.ReadAllAsync())
            {
                Console.WriteLine($"{request.Name}  {request.Surname}");
            }

            return new exampleResponse() { Name = "Mert", Surname = "Başar" };

        }

        public async override Task ServerStreamMethod(exampleRequest request, IServerStreamWriter<exampleTriggerResponse> responseStream, ServerCallContext context)
        {
            var generatedPhotoData = await GeneratePhotoData();
             await Task.Run(async () =>
            {
                for (int i = 0; i < 3000; i++)
                {
                    var data = new exampleTriggerResponse();
                    data.Photo.Add(generatedPhotoData);
                    await responseStream.WriteAsync(data);
                }

            });
        }

        public override async Task<exampleTriggerResponse> UnaryTrigger(emptyMessageForTrigger request, ServerCallContext context)
        {
            try
            {
                var generatedPhotoData = await GeneratePhotoData();
                await Task.Run(() =>
                {
                    for (int i = 0; i < 3000; i++)
                    {
                        mockPhotoList.Add(generatedPhotoData);
                    }
                });

                var response = new exampleTriggerResponse();
                await Task.Run(() => response.Photo.AddRange(mockPhotoList));

                var s = response.CalculateSize();
                return await Task.FromResult(response);

            }
            catch (Exception)
            {

                throw;
            }
        }

        private Task<string> GeneratePhotoData()
        {
            String result = string.Empty;
            for (char j = 'a'; j < 'z'; j++)
            {
                mockPhoto += j;
            }

            for (int i = 0; i < 30000; i++)
            {
                result += mockPhoto;
            }

           return Task.FromResult(result);

        }
    }
}
