using Jammer;

static class Program
{
    static void Main(string[] args) {

        string mutexName = "jammer";

        using Mutex mutex = new Mutex(true, mutexName, out bool createdNew);
        // If the mutex was successfully created, it means this is the first instance
        if (createdNew)
        {
            Console.WriteLine("Launching Jammer...");
            jammer.Start.Run(args);
            // The program run after this point to the end, continuing inside with a new thread
        }
        else
        {
            Console.WriteLine("Jammer is already running.");
            // The program ends here
        }
    }
}