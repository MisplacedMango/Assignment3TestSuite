//Comments written for lines of code written by me, comments for the lines of code are written on top of the code.
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using System.Text.Json;
using System.Runtime.CompilerServices;
using System.Net.Http;

public class Server
{
    private readonly int _port;

    public Server(int port)
    {
        _port = port;


    }


    public void Run()
    {

        var server = new TcpListener(IPAddress.Loopback, _port); // IPv4 127.0.0.1 IPv6 ::1
        server.Start();

        Console.WriteLine($"Server started on port {_port}");

        while (true)
        {
            var client = server.AcceptTcpClient();
            Console.WriteLine("Client connected!!!");
            //Creates a new thread for handling the client connection
            new Thread(() => HandleClient(client)).Start(); 

            try
            {
                var stream = client.GetStream();
                string msg = ReadFromStream(stream);

                Console.WriteLine("Message from client: " + msg);

                WriteToStream(stream, msg.ToUpper());
            }
            catch { }

        }

    }
    // Here we manage the communication with the connected client
    private void HandleClient(TcpClient client)  
    {
        try
        {
            //Get the network stream associated with the client, which allows reading from and writing to the client
            var stream = client.GetStream();
            // Reading incoming requests from the client and store it in the request Json variable
            string requestJson = ReadFromStream(stream); 
            //Log the message received from the client to the console for debugging purposes
            Console.WriteLine("Message from client: " + requestJson); 

            //process requests and getting response in the forms of JSON strings
            string responseJson = RequestProcessor.ProcessRequest(requestJson);
            //Sending the response back to the client through the network stream
            WriteToStream(stream, responseJson); 
        }
        catch (Exception ex)
        {
            //If an error occurs during the handling od the client, log the error message to the console
            Console.WriteLine("Error handling client: " + ex.Message); 
        }
        finally
        {
            //Ensure that the client connection is closed, regardless of whether the previous code succeeded ot failed
            client.Close();
        }

    }


    private string ReadFromStream(NetworkStream stream)
    {
        var buffer = new byte[1024];
        var readCount = stream.Read(buffer);
        return Encoding.UTF8.GetString(buffer, 0, readCount);
    }

    private void WriteToStream(NetworkStream stream, string msg)
    {
        var buffer = Encoding.UTF8.GetBytes(msg);
        stream.Write(buffer);
    }


    //This class defines the structure of a request that the server expects from clients

    public class Request
    {
        //The type of request: Create, Read, Echo, Delete and Update, 
        public string Method { get; set; } 
        // The path for the request, this is not needed for echo type requests.
        public string Path { get; set; } 
        //The date to validate a request in Unix format as set forth by the assignment.
        public string Date { get; set; }
        //The main content of the request, which can be prosed or echoed
        public string Body { get; set; }
    }
    // This class defines the structure of the response that the server sends back to clients
    public class Response
    {
        //The status of the response, such as success or error
        public string Status { get; set; }
        //The content of the response, contains the result of processing the request
        public string Body { get; set; }
    }
    //This class is responsible for processing requests received by the server.
    public class RequestProcessor
    {
        //This method takes a JSON string as input, converts it to a request object, and processes it.
        public static string ProcessRequest(string requestJson)
        {
            //Json string to request object through deserialization
            Request request = JsonSerializer.Deserialize<Request>(requestJson);

            //Validation of request
            if (request == null || string.IsNullOrEmpty(request.Method))
            {
                //If the request is invalid return invalid response
                return ErrorResponse("Invalid Request format");
            }

            //Processing the request according to the called method
            switch (request.Method.ToLower())
            {
                case "update":
                    //Check if the update request is valid; if not, return an error.
                    if (!IsValidUpdateRequest(request))
                    {
                        return ErrorResponse("Invalid update request format! ");
                    }
                    //If valid, return a success response for the update.
                    return SuccessResponse("Update was successful! ");
                case "echo":
                    //For echo requests, return the body of requests back as success response
                    return SuccessResponse(request.Body);

                default:
                    //If method is not recognized, return error response
                    return ErrorResponse("Method not recognized! ");
            }
        }
        //This method checks if the update request has valid information(the type: Create, Read, Echo, Delete and Update( 
        private static bool IsValidUpdateRequest(Request request)
        {
            // validation logic,like checking if the path, date and body are valid.
            return !string.IsNullOrEmpty(request.Path) && IsUnixTime(request.Date) && !string.IsNullOrEmpty(request.Body);
        }
        //This method checks if the provided date string is a valid Unix Time(Time taken since 1970 January 1)
        private static bool IsUnixTime(string date)
        {
            //Try to parse the date string into a long integer
            if (long.TryParse(date, out var unixTime))
            {
                //Converts the unix time into DateTime and compare it with the current time.
                DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime;

                return dateTime <= DateTime.UtcNow;
            }
            return false;
        }
        //This method creates an success response as a JSON string, indicating that the operation was successful. 
        private static string SuccessResponse(string body)
        {
            // Create a new Response object with the status and body content
            Response response = new Response
            {
                Status = "success",
                Body = body
            };
            //Serialize the Response object into a JSON string and return it
            return JsonSerializer.Serialize(response);

        }
        //This method creates an error response as a JSON string, providing information about what went wrong.
        private static string ErrorResponse(string errorMessage)
        {
            //Create a new response object with the error status and message.
            Response response = new Response
            {
                Status = "Error",
                Body = errorMessage
            };
            //Serialize the Response object into a JSON string and return it.
            return JsonSerializer.Serialize(response);


        }
    }
}



