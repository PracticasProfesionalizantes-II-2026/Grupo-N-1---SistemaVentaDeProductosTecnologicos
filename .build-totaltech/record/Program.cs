using Microsoft.Playwright;
using Totaltech.IntegrationTests.E2E;
using System.Diagnostics;
using System.Text.Json;

var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
var output=Path.Combine(root,"output/presentacion-totaltech-final");
Directory.CreateDirectory(Path.Combine(output,"grabacion"));
var fixture=new CatalogoE2EFixture();
var report=new List<object>();
IBrowserContext? context=null;
IPage? page=null;
string? state=null;
async Task Pause(int ms=1400)=> await Task.Delay(ms);
async Task Segment(string name,Func<Task> action){
 Console.WriteLine("PASO: "+name);
 var sw=Stopwatch.StartNew();await action();
 report.Add(new {name,success=true,seconds=sw.Elapsed.TotalSeconds});
 Console.WriteLine("OK: "+name);
}
async Task Login(IPage pg,string email,string password){
 await pg.GotoAsync(fixture.FrontendUrl+"/Home/Login");
 await pg.GetByLabel("Email").FillAsync(email);await pg.GetByLabel("Contraseña",new(){Exact=true}).FillAsync(password);
 await pg.GetByRole(AriaRole.Button,new(){Name="Iniciar sesión",Exact=true}).ClickAsync();await Pause();
}
async Task OpenRecording(string name,string? storage=null){
 context=await fixture.Browser.NewContextAsync(new(){ViewportSize=new(){Width=1440,Height=900},RecordVideoDir=Path.Combine(output,"grabacion"),RecordVideoSize=new(){Width=1440,Height=900},StorageState=storage});
 page=await context.NewPageAsync();page.SetDefaultTimeout(12000);
}
async Task CloseRecording(string name){
 var video=page!.Video!;await context!.CloseAsync();await video.SaveAsAsync(Path.Combine(output,"grabacion",name+".webm"));await video.DeleteAsync();
 context=null;page=null;
}
try {
 Console.WriteLine("Inicializando base desechable TotaltechTests_<GUID> y servicios aislados.");
 await fixture.InitializeAsync();
 var admin=await fixture.CrearAdministradorAsync();
 var email="demo-"+Guid.NewGuid().ToString("N")[..8]+"@example.com";var password="DemoTotalTech2026!";
 await OpenRecording("01-cliente");
 await Segment("Registro UC-01",async()=>{
  await page!.GotoAsync(fixture.FrontendUrl+"/Home/Register");await Pause();
  await page.GetByRole(AriaRole.Button,new(){Name="Crear mi cuenta"}).ClickAsync();await Pause();
  foreach(var v in new[]{("Nombre","Cliente"),("Apellido","Demostración"),("Email",email),("Telefono","1111111111"),("Contrasena",password),("ConfirmarContrasena",password)}){await page.Locator("#"+v.Item1).FillAsync(v.Item2);await Pause(250);}
  await page.GetByRole(AriaRole.Button,new(){Name="Crear mi cuenta"}).ClickAsync();await Pause();
  if(await fixture.ObtenerUsuarioAsync(email) is null)throw new Exception("Registro sin persistencia");
 });
 await Segment("Acceso UC-02",async()=>{await Login(page!,email,password);state=await context!.StorageStateAsync();});
 await Segment("Búsqueda UC-08",async()=>{
  await page!.GotoAsync(fixture.FrontendUrl+"/Productos");await Pause();
  await page.Locator("#categorias").GetByRole(AriaRole.Link,new(){Name="Notebooks",Exact=true}).ClickAsync();await Pause();
  await page.Locator("#texto").FillAsync("notebook");await page.GetByLabel("Sólo disponibles").CheckAsync();
  await page.Locator("#precioMin").FillAsync("1000000");await page.Locator("#precioMax").FillAsync("2000000");await Pause();
  await page.GetByRole(AriaRole.Button,new(){Name="Aplicar",Exact=true}).ClickAsync();await Pause();
  if(await page.Locator(".catalogo__grilla article").CountAsync()==0)throw new Exception("Catálogo vacío");
  await page.Locator("#texto").FillAsync("no-existe-demostracion");await page.GetByRole(AriaRole.Button,new(){Name="Aplicar",Exact=true}).ClickAsync();await Pause();
  await Assertions.Expect(page.GetByText("No hay productos que coincidan")).ToBeVisibleAsync();
  await page.GetByRole(AriaRole.Link,new(){Name="Limpiar filtros"}).ClickAsync();await Pause();
  await page.Locator("#texto").FillAsync("Notebook Lenovo Yoga 7");await page.GetByRole(AriaRole.Button,new(){Name="Aplicar",Exact=true}).ClickAsync();
 });
 await Segment("Detalle UC-09",async()=>{await page!.Locator(".catalogo__grilla article").First.GetByRole(AriaRole.Link,new(){Name="Ver producto"}).ClickAsync();await Pause(2500);await page.EvaluateAsync("window.scrollBy(0,250)");await Pause();});
 await Segment("Carrito RF3",async()=>{
  await page!.GetByRole(AriaRole.Button,new(){Name="Agregar al carrito"}).ClickAsync();await Pause();
  await page.Locator("input[name=cantidad]").FillAsync("2");await page.GetByRole(AriaRole.Button,new(){Name="Actualizar",Exact=true}).ClickAsync();await Pause();
  await Assertions.Expect(page.Locator(".cart-badge")).ToHaveTextAsync("2");
  await page.GetByRole(AriaRole.Button,new(){Name="Eliminar",Exact=true}).ClickAsync();await Pause();await Assertions.Expect(page.GetByText("Tu carrito está vacío.")).ToBeVisibleAsync();
 });
 await CloseRecording("01-Cliente-registro-catalogo-carrito");
 await OpenRecording("02-admin");await Login(page!,admin.Email,admin.Contrasena);
 await Segment("ABM UC-19",async()=>{
  await page!.GotoAsync(fixture.FrontendUrl+"/Productos/Crear");await Pause();
  await page.Locator("#Nombre").FillAsync("Producto de demostración TotalTech");await page.Locator("#Descripcion").FillAsync("Producto ficticio para presentar el sistema");await page.Locator("#Precio").FillAsync("150000");await page.Locator("#Stock").FillAsync("10");
  await page.Locator("#IdCategoria").SelectOptionAsync(new SelectOptionValue(){Index=1});await page.Locator("#IdProveedor").SelectOptionAsync(new SelectOptionValue(){Index=1});
  await page.GetByRole(AriaRole.Button,new(){Name="Guardar",Exact=true}).ClickAsync();await Pause();
  await page.GotoAsync(fixture.FrontendUrl+"/Productos?texto=Producto%20de%20demostración%20TotalTech");
  await page.Locator(".catalogo__grilla article").GetByRole(AriaRole.Link,new(){Name="Editar",Exact=true}).ClickAsync();await Pause();
  await page.Locator("#Precio").FillAsync("145000");await page.Locator("#Descripcion").FillAsync("Oferta actualizada desde la administración");
  await page.GetByLabel("Imagen de referencia",new(){Exact=true}).SetInputFilesAsync(Path.Combine(root,"Frontend/wwwroot/images/totaltech-logo.png"));await Pause();
  await page.GetByRole(AriaRole.Button,new(){Name="Guardar cambios",Exact=true}).ClickAsync();await Pause();
  await Assertions.Expect(page.GetByText("Producto actualizado correctamente.",new(){Exact=true})).ToBeVisibleAsync();
  await using(var guest=await fixture.Browser.NewContextAsync()) {var pg=await guest.NewPageAsync();await pg.GotoAsync(fixture.FrontendUrl+"/Productos?texto=Producto%20de%20demostración%20TotalTech");if(await pg.Locator(".catalogo__grilla article").CountAsync()!=1)throw new Exception("Producto no publicado");}
  page.Dialog+=async (_,dialog)=>await dialog.AcceptAsync();
  await page.Locator(".catalogo__grilla article").GetByRole(AriaRole.Button,new(){Name="Eliminar",Exact=true}).ClickAsync();await Pause();
  if(await page.Locator(".catalogo__grilla article").CountAsync()!=0)throw new Exception("Producto no eliminado");
 });
 await CloseRecording("02-Administrador-productos");
 await OpenRecording("03-calidad",state);
 await Segment("Compatibilidad y permisos",async()=>{
  await page!.SetViewportSizeAsync(390,844);await page.GotoAsync(fixture.FrontendUrl+"/Productos");await Pause(2500);await page.EvaluateAsync("window.scrollBy(0,550)");await Pause(2000);
  if(!await page.Locator("body").EvaluateAsync<bool>("el=>el.scrollWidth<=el.clientWidth"))throw new Exception("Desborde móvil");
  await page.SetViewportSizeAsync(1440,900);var denied=await page.GotoAsync(fixture.FrontendUrl+"/Productos/Crear");await Pause();
  if(denied is null || denied.Status!=403)throw new Exception("Se esperaba HTTP 403 para cliente");
  if(await page.Locator("#Stock").CountAsync()>0)throw new Exception("Cliente accede a administración");
  await page.GotoAsync(fixture.FrontendUrl+"/Productos");await Pause(2500);
 });
 await CloseRecording("03-Movil-y-permisos");
 Console.WriteLine("RECORRIDO COMPLETO");
} catch(Exception ex){Console.WriteLine("ERROR: "+ex.Message);report.Add(new{success=false,error=ex.Message});if(page!=null)await page.ScreenshotAsync(new(){Path=Path.Combine(root,".build-totaltech/record-error.png"),FullPage=true});if(context!=null)await context.CloseAsync();Environment.ExitCode=1;}
finally{await fs();await fixture.DisposeAsync();}
async Task fs()=>await File.WriteAllTextAsync(Path.Combine(root,".build-totaltech/demo-report.json"),JsonSerializer.Serialize(report,new JsonSerializerOptions(){WriteIndented=true}));
