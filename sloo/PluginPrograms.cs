using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;

namespace sloo
{
    class PluginPrograms : VstPluginProgramsBase
    {
        Plugin _plugin;

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="plugin">Must not be null.</param>
        public PluginPrograms(Plugin plugin)
        {
            _plugin = plugin;
        }

        /// <summary>
        /// Initializes the plugin program collection.
        /// </summary>
        /// <returns>A filled program collection.</returns>
        protected override VstProgramCollection CreateProgramCollection()
        {
            VstProgramCollection programs = new VstProgramCollection();

            VstProgram prog = new VstProgram(_plugin.Model.Categories);
            prog.Name = "SLoOultraHD";
            _plugin.Model.CreateParameters(prog.Parameters);
            
            programs.Add(prog);

            return programs;
        }
    }
}