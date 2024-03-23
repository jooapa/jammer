using jammer;


class Program
{
    public static void Main(string[] args)
    {
        string mutexName = "jammer";

        using (Mutex mutex = new Mutex(true, mutexName, out bool createdNew))
        {

            // If the mutex was successfully created, it means this is the first instance
            if (createdNew)
            {
                Console.WriteLine("Launching Jammer...");
                Start.Run(args);
            }
        }
    }
}

