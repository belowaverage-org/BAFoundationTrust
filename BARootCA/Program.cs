using System.Reflection;
using System.Security.Cryptography.X509Certificates;

Stream RootCA = Assembly.GetExecutingAssembly().GetManifestResourceStream("BABootstrap.below-average-Root-CA.crt")!;
StreamReader sr = new(RootCA);


/*var CAStore = new X509Store(StoreName.Root);
CAStore.Open(OpenFlags.ReadOnly);
foreach (X509Certificate cert in CAStore.Certificates)
{
    if (cert.GetRawCertData().To
}*/