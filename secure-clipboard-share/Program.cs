using System.Diagnostics;
using System.Text;
using System.Text.Json;

string homedir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
Console.WriteLine(homedir);
string ecSpDir = Path.Combine(homedir, ".everycom", "secure-clipboard-share");
Directory.CreateDirectory(ecSpDir);


string serverurl = Path.Combine(ecSpDir, "serverurl");
if (!File.Exists(serverurl))
{
    Console.WriteLine("serverurl not found");
    Environment.Exit(1);
    return;
}
serverurl = File.ReadAllLines(serverurl)[0];
string scskey = Path.Combine(ecSpDir, "scskey");
if (!File.Exists(scskey))
{
    Console.WriteLine("scskey not found");
    Environment.Exit(1);
    return;
}
scskey = File.ReadAllLines(scskey)[0];
using HttpClient client = new();
client.DefaultRequestHeaders.Add("User-Agent", "curl/7.81.0");

string? newtext = null;
if (args.Length != 0 && args[0] == "w")
{
    using Process p = new Process();
    p.StartInfo.UseShellExecute = false;
    p.StartInfo.RedirectStandardOutput = true;
    if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
    {
        Console.WriteLine("ON WINDOWS");
        p.StartInfo.FileName = "powershell.exe";
        p.StartInfo.ArgumentList.Add("-command");
        p.StartInfo.ArgumentList.Add("gcb");
    }
    else
    {
        Console.WriteLine("NOT ON WINDOWS");
        p.StartInfo.FileName = "xclip";
        p.StartInfo.ArgumentList.Add("-o");
        p.StartInfo.ArgumentList.Add("-selection");
        p.StartInfo.ArgumentList.Add("clipboard");
    }
    p.Start();
    newtext = p.StandardOutput.ReadToEnd();
    p.WaitForExit();
}
var reqobj = new Req(scskey, newtext);
string reqstr = JsonSerializer.Serialize(reqobj);
//Console.WriteLine(serverurl);
//Console.WriteLine(reqstr);
using StringContent jsonContent = new(reqstr, Encoding.UTF8, "application/json");

using HttpResponseMessage hrm = await client.PostAsync(serverurl, jsonContent);
await using Stream stream = await hrm.Content.ReadAsStreamAsync();
JsonElement result = (await JsonSerializer.DeserializeAsync<JsonElement>(stream));
JsonElement jebuf;
if (!result.TryGetProperty("success", out jebuf))
{
    throw new Exception("Unexpected exception");
}
//Console.WriteLine(jebuf.GetRawText());
if ("true" != jebuf.GetRawText())
{
    throw new Exception("Unexpected failure");
}
if (newtext != null)
{
    Console.WriteLine("WRITTEN");
    return;
}
if (!result.TryGetProperty("results", out jebuf))
{
    throw new Exception("Unexpected exception");
}
JsonElement resstr;
if (!jebuf[0].TryGetProperty("v", out resstr))
{
    throw new Exception("Unexpected exception");
}

if (args.Length != 0 && args[0] == "r")
{
    Console.WriteLine(resstr.GetRawText());
}

string resultstr = resstr.GetString() ?? "";
{
    using Process p = new Process();
    p.StartInfo.UseShellExecute = false;
    p.StartInfo.RedirectStandardInput = true;
    if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
    {
        Console.WriteLine("TRYING CLIP");
        p.StartInfo.FileName = "clip.exe";
    }
    else
    {
        Console.WriteLine("TRYING XCLIP");
        p.StartInfo.FileName = "xclip";
        p.StartInfo.ArgumentList.Add("-selection");
        p.StartInfo.ArgumentList.Add("clipboard");
    }
    p.Start();
    {
        using StreamWriter stdinsw = p.StandardInput;
        stdinsw.Write(resultstr);
        stdinsw.Close();
    }
    p.WaitForExit();
}
Console.WriteLine("DONE");

