<Query Kind="Program" />

void Main()
{
	var solutionDirectory = Path.GetDirectoryName(Util.CurrentQueryPath);
	var ignoredExtensions = new [] {".csproj", ".config", ".md", ".xml", ".ico", ".resx", ".settings", ".png", ".pfx", ".appxmanifest", ".exe", ".bat", ".js", ".map", ".pubxml", ".plist", ".json", ".txt", ""};
	
	var filesPerProject = (from dir in Directory.EnumerateDirectories(solutionDirectory, "Adaptive.*")
				from file in Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories)
				let extension = new FileInfo(file).Extension
				let dirName = new DirectoryInfo(dir).Name
				where !ignoredExtensions.Contains(extension)
				   && !file.Contains("\\obj\\") && !file.Contains("\\bin\\") && !file.Contains(@"Designer\Design") && !file.Contains(@"AssemblyInfo.cs") 
				   && !file.Contains(@".Designer.cs") && !file.Contains(@".d.ts") && !file.Contains(@"\Scripts\") && !file.Contains(@"Properties\Annotations.cs") && !file.Contains(@"\GridSplitter") && !file.Contains(@".designer.cs")
				   && !dirName.Contains("Adaptive.ReactiveTrader.Server") && !dirName.Contains("Adaptive.ReactiveTrader.ControlClient.CLI") 
				   && !dirName.Contains("Adaptive.ReactiveTrader.Tests") && !dirName.Contains("Adaptive.ReactiveTrader.ControlClient.GUI")
				group file by dirName into filesByDir
				select new RawProjectStats()
					{ 
						Name = filesByDir.Key.Replace("Adaptive.ReactiveTrader.", string.Empty),
						Files = filesByDir.Select(f => new FileStats() 
														{ 
															Name = f, 
															Extension = new FileInfo(f).Extension, 
															Lines = File.ReadLines(f).Count() 
														}
												  ).OrderByDescending(f => f.Lines)
					}).ToList();
	
	var projectStats = filesPerProject.Select(rawProjectStats => rawProjectStats.GetStats()).ToDictionary(s => s.Project);
	
	GetClientStats(projectStats).Dump();
	
	filesPerProject.Dump();	
}

IEnumerable<ClientStats> GetClientStats(Dictionary<string, ProjectStats> projectStats)
{
	yield return new ClientStats()
	{
		Client = "WPF",
		SharedCode = projectStats["Client"].CodeLines + projectStats["Client.Domain"].CodeLines + projectStats["Shared"].CodeLines,
		UnsharedCode = projectStats["Client.GUI"].CodeLines,
		UICode = projectStats["Client.GUI"].UILines
	};
	
	yield return new ClientStats()
	{
		Client = "Windows App Store",
		SharedCode = projectStats["Client"].CodeLines + projectStats["Client.Domain"].CodeLines + projectStats["Shared"].CodeLines,
		UnsharedCode = projectStats["Client.WindowsStoreApp"].CodeLines,
		UICode = projectStats["Client.WindowsStoreApp"].UILines
	};
	
	yield return new ClientStats()
	{
		Client = "iOS",
		SharedCode = projectStats["Client.Domain"].CodeLines + projectStats["Shared"].CodeLines,
		UnsharedCode = projectStats["Client.iOSTab"].CodeLines,
		UICode = projectStats["Client.iOSTab"].UILines
	};
	
	yield return new ClientStats()
	{
		Client = "HTML5",
		SharedCode = 0,
		UnsharedCode = projectStats["Web"].CodeLines,
		UICode = projectStats["Web"].UILines
	};
}


class FileStats {
	public string Name { get; set; }
	public string Extension { get; set; }
	public int Lines { get; set; }
}

class RawProjectStats {
	public string Name { get; set; }
	public IEnumerable<FileStats> Files {get; set; }
	
	public ProjectStats GetStats() 
	{
		var codeExtensions = new []{ ".cs", ".ts"};
		var uiExtensions = new []{ ".xaml", ".xib", ".aspx", ".css"};
	
		var filesWithInvalidExtensions = Files.Where(f => !codeExtensions.Contains(f.Extension) && !uiExtensions.Contains(f.Extension)).Select(f => f.Name).ToList();
	
		if(filesWithInvalidExtensions.Any()) {
			throw new Exception("Extensions are not associated with UI or Code metrics: " + string.Join(", ", filesWithInvalidExtensions));
		}	
	
		var codeLines = Files.Where(f => codeExtensions.Contains(f.Extension)).Sum(f => f.Lines);
		var uiLines = Files.Where(f => uiExtensions.Contains(f.Extension)).Sum(f => f.Lines);
	
		return new ProjectStats()
		{
			Project = Name,
			UILines = uiLines,
			CodeLines = codeLines
		};
	}
}

class ProjectStats {
	public string Project { get; set; }
	public int UILines { get; set; }
	public int CodeLines { get; set; }
}

class ClientStats
{
	public string Client { get; set; }
	public int UICode { get; set; }
	public int SharedCode { get; set; }
	public int UnsharedCode { get; set; }
}




