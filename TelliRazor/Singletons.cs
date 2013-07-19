
namespace TelliRazor
{
	public static class Singletons
	{
		public static IRazorWidgetFileService FileService = new RazorWidgetFileService();
		public static IRazorWidgetService WidgetService = new RazorWidgetService(FileService); 

	}
}
