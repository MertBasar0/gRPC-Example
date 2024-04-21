using Google.Protobuf;
using Google.Protobuf.Collections;
using Grpc.Core;

namespace gRPCExample.Services
{
    public class ExampleService : gRPCExample.ExampleRPCServices.ExampleRPCServicesBase
    {
        public string mockPhoto { get; set; } = string.Empty;
        private int mockPhotoLenght { get; set; }
        public List<string> mockPhotoList { get; set; }

        public ExampleService()
        { 
            mockPhotoList = new();
        }

        //public override async Task<exampleResponse> UnaryMethod(exampleRequest request, ServerCallContext context)
        //{
        //     GeneratePhotoData();
        //    return await Task.FromResult(new exampleResponse()
        //    {
        //        Name = request.Name,
        //        Surname = request.Surname
        //    });
        //}

        public override Task BidirectionalStreamingMethod(IAsyncStreamReader<exampleRequest> requestStream, IServerStreamWriter<exampleResponse> responseStream, ServerCallContext context)
        {
            return base.BidirectionalStreamingMethod(requestStream, responseStream, context);
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
                    data.Photo.Add(generatedPhotoData + $"\n\n\n\n\n\n{i}");
                    await responseStream.WriteAsync(data);
                }

            });

            //var response = new exampleTriggerResponse();
            //await Task.Run(() => response.Photo.AddRange(mockPhotoList));

            //var s = response.CalculateSize();

            //foreach (var photo in mockPhotoList)
            //{
            //    await responseStream.WriteAsync(response);

            //}

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
                mockPhotoLenght = result.Length;
            }

           return Task.FromResult(result);

        }
    }
}
