namespace MyWinFormsApp;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Form f=new MainForm();
        f.Size=new Size(800,600);
        // Test 
        Application.Run(new MainForm());

    }    
}

