open System
open System.IO
open System.Net.Sockets
type Client() =
    let inStream = new StreamReader(Console.OpenStandardInput())
    let outStream = new StreamWriter(Console.OpenStandardOutput())
    
    member this.Log (s: string) =
        Console.WriteLine(s)

    member this.ConnectToServer(serverURL: string, port: int) =
        let clientSocket = new TcpClient(serverURL, port)
        let networkStream = clientSocket.GetStream()
        let inStream = new StreamReader(networkStream)
        let outStream = new StreamWriter(networkStream)

        let greeting = inStream.ReadLine()
        // this.Log("Received: " + greeting)

        (clientSocket, inStream, outStream)

    member this.Run() =
        this.Log("Client's inputs and outputs (Client Terminal):")
        this.Log("")
        let args = System.Environment.GetCommandLineArgs()
        let serverURL = "127.0.0.1"
        let port = if args.Length > 1 then int args.[1] else 9659

        let mutable (clientSocket, inStream, outStream) = this.ConnectToServer(serverURL, port)

        while true do
            
            let input = Console.ReadLine()
            outStream.WriteLine(input)
            outStream.Flush()
            this.Log("Sending Command: " + input)

            try
                let response = inStream.ReadLine()

                match response with
                | "-1" -> this.Log("Server response: incorrect operation command")
                | "-2" -> this.Log("Server response: number of inputs is less than two")
                | "-3" -> this.Log("Server response: number of inputs is more than four")
                | "-4" -> this.Log("Server response: one or more of the inputs contain(s) non-number(s)")
                | "-5" -> 
                    this.Log("exit")
                    // Close the client socket and exit gracefully
                    clientSocket.Close()
                    Environment.Exit(0)
                | _ -> this.Log("Server response: " + response)
            with
            | :? IOException as ex ->
                this.Log("All process are Terminated")
                clientSocket.Close()
                Environment.Exit(1)


[<EntryPoint>]
let main(args : string[]) =
    let client = Client()
    client.Run()
    0 // Return an integer exit code
