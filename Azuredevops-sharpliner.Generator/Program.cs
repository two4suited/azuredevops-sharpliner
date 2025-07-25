using Sharpliner.AzureDevOps;
using Azuredevops_sharpliner.Pipelines;
using System.Reflection;

namespace Azuredevops_sharpliner.Generator;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 Azure DevOps Pipeline Generator");
        Console.WriteLine("==================================");
        
        try
        {
            // Create .azdo directory if it doesn't exist
            var azdoDir = Path.Combine(Directory.GetCurrentDirectory(), ".azdo");
            if (!Directory.Exists(azdoDir))
            {
                Directory.CreateDirectory(azdoDir);
                Console.WriteLine($"📁 Created directory: {azdoDir}");
            }
            
            // Set the output directory for Sharpliner
            System.Environment.SetEnvironmentVariable("SHARPLINER_OUTPUT_DIR", azdoDir);
            
            var generatedFiles = new List<string>();
            
            // Generate all pipelines (default behavior)
            Console.WriteLine("🔄 Generating all pipeline definitions...");
            
            // Generate Build Pipeline
            var buildPipeline = new DotNetBuildPipeline("azure-pipelines.yml");
            await GeneratePipeline(buildPipeline, azdoDir, generatedFiles);
            
            // Generate PR Pipeline
            var prPipeline = new DotNetPRPipeline("azure-pipelines-pr.yml");
            await GeneratePipeline(prPipeline, azdoDir, generatedFiles);
            
            Console.WriteLine();
            Console.WriteLine("📊 Generation Summary");
            Console.WriteLine("====================");
            Console.WriteLine($"📁 Output Directory: {azdoDir}");
            Console.WriteLine($"📄 Files Generated: {generatedFiles.Count}");
            
            if (generatedFiles.Any())
            {
                Console.WriteLine("\n📋 Generated Files:");
                foreach (var file in generatedFiles)
                {
                    var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), file);
                    var fileSize = new FileInfo(file).Length;
                    Console.WriteLine($"   • {relativePath} ({fileSize:N0} bytes)");
                }
            }
            
            Console.WriteLine();
            Console.WriteLine("🎉 Pipeline generation completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Fatal error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            System.Environment.Exit(1);
        }
    }
    
    private static async Task GeneratePipeline(object pipelineInstance, string azdoDir, List<string> generatedFiles)
    {
        try
        {
            string targetFile;
            string yamlContent;
            string pipelineName;
            
            if (pipelineInstance is PipelineDefinition pipeline)
            {
                targetFile = pipeline.TargetFile;
                yamlContent = pipeline.Serialize();
                pipelineName = pipeline.GetType().Name;
            }
            else if (pipelineInstance is SingleStagePipelineDefinition singleStagePipeline)
            {
                targetFile = singleStagePipeline.TargetFile;
                yamlContent = singleStagePipeline.Serialize();
                pipelineName = singleStagePipeline.GetType().Name;
            }
            else
            {
                throw new ArgumentException("Unknown pipeline type");
            }
            
            Console.WriteLine($"🔨 Processing: {pipelineName}");
            
            var outputPath = Path.Combine(azdoDir, targetFile);
            
            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            
            // Write the YAML file
            await File.WriteAllTextAsync(outputPath, yamlContent);
            generatedFiles.Add(outputPath);
            
            Console.WriteLine($"✅ Generated: {Path.GetRelativePath(Directory.GetCurrentDirectory(), outputPath)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error processing pipeline: {ex.Message}");
        }
    }
}
