using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

Stream rootCAStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BAFoundation.below-average-Root-CA.crt")!;
byte[] rootCABytes = new byte[rootCAStream.Length];
rootCAStream.ReadExactly(rootCABytes, 0, (int)rootCAStream.Length);
X509Certificate2 rootCACert = X509CertificateLoader.LoadCertificate(rootCABytes);
var caStore = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
caStore.Open(OpenFlags.ReadOnly);

bool rootFound = false;
foreach (X509Certificate2 cert in caStore.Certificates)
{
    if (cert.Thumbprint != rootCACert.Thumbprint) continue;
    if (cert.RawData.SequenceEqual(rootCACert.RawData))
    {
        rootFound = true;
        break;
    }
}
caStore.Close();

if (!rootFound)
{
    try
    {
        caStore.Open(OpenFlags.ReadWrite);
        caStore.Add(rootCACert);
        caStore.Close();
    }
    catch
    {
        ProcessStartInfo psiRelaunch = new()
        {
            FileName = Environment.ProcessPath,
            WorkingDirectory = Environment.CurrentDirectory,
            UseShellExecute = true,
            Verb = "RunAs"
        };
        try
        {
            Process.Start(psiRelaunch)?.WaitForExit();
        }
        catch { }
    }
}

if (args.Length == 0) return 0;

if (!File.Exists(args[0])) return 1000;

X509Certificate2 signatureCert;

try
{
    #pragma warning disable SYSLIB0057
    signatureCert = new(args[0]);
    #pragma warning restore SYSLIB0057
}
catch
{
    return 1001;
}

X509Chain chain = new();
chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
chain.ChainPolicy.VerificationFlags = X509VerificationFlags.IgnoreNotTimeValid | X509VerificationFlags.IgnoreEndRevocationUnknown;
chain.ChainPolicy.UrlRetrievalTimeout = TimeSpan.FromSeconds(3);
chain.ChainPolicy.CustomTrustStore.Add(rootCACert);
    
if (!chain.Build(signatureCert)) return 1002;

if (!chain.ChainElements[^1].Certificate.RawData.SequenceEqual(rootCACert.RawData)) return 1003;

ProcessStartInfo psi = new()
{
    FileName = args[0],
    Arguments = string.Join(' ', args[1..]),
    WorkingDirectory = Environment.CurrentDirectory,
    UseShellExecute = true
};
try
{
    Process.Start(psi);
}
catch 
{
    return 1004;
}

return 0;