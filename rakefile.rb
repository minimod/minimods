require 'rake'

desc "Done..."
task:default => [:licence] do
end

task:licence do
	puts "Adding licence information to code files."
	Dir.glob('./**/*minimod.cs').each do|f|
    code = File.read f
    if uu = code =~ /public class|public static class|internal static class/
      puts uu
    end
  end
end
