using Quartz;
using Quartz.Spi;

namespace VivesBankApi.Rest.Movimientos.Jobs;

/// <summary>
    /// Implementación de <see cref="IJobFactory"/> que permite la creación de trabajos con un ámbito de servicio (scope).
    /// </summary>
    /// <remarks>
    /// Esta clase crea una nueva instancia de un trabajo (<see cref="IJob"/>) por cada ejecución, utilizando un 
    /// ámbito de servicio (scope) para inyectar las dependencias adecuadas, garantizando que los servicios sean 
    /// creados con el ciclo de vida correcto.
    /// </remarks>
    /// <author>VivesBank Team</author>
    public class ScopedJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Constructor que inicializa la fábrica de trabajos con un proveedor de servicios.
        /// </summary>
        /// <param name="serviceProvider">El proveedor de servicios utilizado para crear el ámbito de servicio.</param>
        /// <remarks>
        /// Este constructor inyecta el <see cref="IServiceProvider"/> que se utilizará para crear el ámbito y resolver 
        /// las dependencias de los trabajos cuando se invoca <see cref="NewJob"/>.
        /// </remarks>
        public ScopedJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Crea un nuevo trabajo para el disparo de un <see cref="IJob"/>.
        /// </summary>
        /// <param name="bundle">El paquete de información sobre el disparo del trabajo, contiene detalles del trabajo.</param>
        /// <param name="scheduler">El programador que ejecuta el trabajo.</param>
        /// <returns>Una nueva instancia de <see cref="IJob"/> para ejecutarse.</returns>
        /// <remarks>
        /// Este método crea un nuevo ámbito de servicio, resuelve el tipo de trabajo y lo devuelve.
        /// Es importante que cada trabajo tenga su propio ámbito de servicio para evitar problemas con 
        /// el ciclo de vida de las dependencias.
        /// </remarks>
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            // Crear un ámbito de servicio para este trabajo
            using var scope = _serviceProvider.CreateScope();
            // Resolver y devolver el trabajo del tipo adecuado
            return (IJob)scope.ServiceProvider.GetRequiredService(bundle.JobDetail.JobType);
        }

        /// <summary>
        /// Devuelve el trabajo después de que haya sido ejecutado.
        /// </summary>
        /// <param name="job">El trabajo que se ha ejecutado y que se está devolviendo.</param>
        /// <remarks>
        /// Este método se utiliza para la limpieza de los trabajos, aunque en este caso no realiza ninguna acción
        /// ya que no es necesario devolver nada al contenedor de dependencias (el trabajo se descarta automáticamente).
        /// </remarks>
        public void ReturnJob(IJob job)
        {
            // No es necesario implementar ninguna lógica aquí para devolver el trabajo,
            // ya que el ciclo de vida del trabajo es manejado por el contenedor de dependencias.
        }
    }