open System
open System.IO
open System.Net
open System.Net.Sockets

let delims = "[ ]+"
let commands = ["add"; "multiply"; "subtract"; "bye"; "terminate"]
let mutable doneFlag = false
let mutable count = 0

let log (s: string) =
    Console.WriteLine(s)

// Test if token is a number
let isNumeric (str: string) =
    try
        let n = Int32.Parse(str)
        true
    with
    | :? FormatException -> false

let tokensAreNumeric (tokens: string[]) =
    tokens.[1..]
    |> Array.forall isNumeric

// Calculates answer given an array of tokens.
// The first token should specify the operation to perform.
let calculateAnswer (tokens: string[]) =
    let mutable answer = Int32.Parse(tokens.[1])
    match tokens.[0].ToLower() with
    | "add" ->
        for i in 2..tokens.Length - 1 do
            answer <- answer + Int32.Parse(tokens.[i])
    | "multiply" ->
        for i in 2..tokens.Length - 1 do
            answer <- answer * Int32.Parse(tokens.[i])
    | "subtract" ->
        for i in 2..tokens.Length - 1 do
            answer <- answer - Int32.Parse(tokens.[i])
    | _ -> ()
    answer

// Parses tokens and generates appropriate error codes or
// calls to calculate the answer.
let parseTokens (tokens: string[]) =
    tokens.[0] <- tokens.[0].ToLower()
    match tokens.[0] with
    | "bye" -> -5
    | "terminate" -> -5
    | cmd when not (List.contains cmd commands) -> -1
    | _ when tokens.Length <= 2 -> -2
    | _ when tokens.Length > 5 -> -3
    | _ when not (tokensAreNumeric tokens) -> -4
    | _ -> calculateAnswer tokens

// Handle a single client asynchronously
let handleClientAsync (client: TcpClient) =
    count <- count + 1
    async {
        use reader = new StreamReader(client.GetStream())
        use writer = new StreamWriter(client.GetStream())
        writer.AutoFlush <- true
        writer.WriteLine(" ")

        try
            while not doneFlag do
                let input = reader.ReadLine()
                if String.IsNullOrWhiteSpace(input) then
                    ignore input // Skip empty lines
                elif input.ToLower() = "terminate" then
                    doneFlag <- true
                    log(sprintf "Received: terminate")
                    log(sprintf "Responding to client with result: -5")
                    writer.WriteLine(-5)
                else
                    // Get answer by parsing tokens and evaluating
                    let tokens = input.Split(delims.ToCharArray())
                    let answer = parseTokens tokens
                    log(sprintf "Received: %s" input)
                    log(sprintf "Responding to client %d with result: %d" count answer)
                    writer.WriteLine(answer)
        finally
            // Close the client socket when done or on exceptions
            // log ("Responding to client with result: -5")
            writer.Close()
            reader.Close()
            client.Close()
            Environment.Exit(0)
    } |> Async.Start


// Runs the Server
let main (args: string[]) =
    let port = if args.Length > 0 then Int32.Parse(args.[0]) else 9659
    let listener = new TcpListener(IPAddress.Any, port)
    log "Server inputs and outputs (Server Terminal):"
    log " "

    printfn "Server is running and listening on port %d" port
    let rec serverLoop () =
        let client = listener.AcceptTcpClient()

        try
            handleClientAsync client
        finally
            if not doneFlag then serverLoop ()

    try
        listener.Start()
        serverLoop ()
    finally
        // If done communicating, close the server
        log "Closing listener."
        listener.Stop()

    0 // Return an integer exit code

[<EntryPoint>]
let mainEntryPoint(args: string[]) =
    main args
