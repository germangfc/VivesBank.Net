using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Exceptions;
using VivesBankApi.Rest.Users.Models;


namespace VivesBankApi.Rest.Users.Mapper
{
    /// <summary>
    /// Clase de utilidad para mapear objetos entre diferentes tipos relacionados con usuarios.
    /// </summary>
    public static class UserMapper
    {
        /// <summary>
        /// Mapea un objeto de tipo <see cref="User"/> a un objeto de tipo <see cref="UserResponse"/>.
        /// </summary>
        /// <param name="user">El objeto de tipo <see cref="User"/> a mapear.</param>
        /// <returns>Un objeto de tipo <see cref="UserResponse"/> que representa al usuario.</returns>
        public static UserResponse ToUserResponse(this User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Dni = user.Dni,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt.ToLocalTime(),
                UpdatedAt = user.UpdatedAt.ToLocalTime(),
                IsDeleted = user.IsDeleted
            };
        }

        /// <summary>
        /// Mapea un objeto de tipo <see cref="UserUpdateRequest"/> a un objeto de tipo <see cref="User"/>.
        /// Actualiza un usuario existente con los datos del request.
        /// </summary>
        /// <param name="request">El objeto de tipo <see cref="UserUpdateRequest"/> que contiene los datos a actualizar.</param>
        /// <param name="existingUser">El usuario existente que será actualizado.</param>
        /// <returns>El objeto <see cref="User"/> actualizado.</returns>
        /// <exception cref="InvalidRoleException">Lanzada cuando el rol especificado no es válido.</exception>
        public static User UpdateUserFromInput(this UserUpdateRequest request, User existingUser)
        {
            User user = existingUser;

            if (request.Dni != null)
            {
                user.Dni = request.Dni;
            }

            if (request.Password != null)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            if (request.IsDeleted != null)
            {
                user.IsDeleted = request.IsDeleted;
            }

            if (request.Role != null)
            {
                if (Enum.TryParse<Role>(request.Role.Trim(), true, out var userRole))
                {
                    user.Role = userRole;
                }
                else
                {
                    throw new InvalidRoleException(request.Role);
                }
            }

            user.UpdatedAt = DateTime.UtcNow;
            return user;
        }

        /// <summary>
        /// Mapea un objeto de tipo <see cref="LoginRequest"/> a un objeto de tipo <see cref="User"/>.
        /// </summary>
        /// <param name="request">El objeto de tipo <see cref="LoginRequest"/> con los datos de login.</param>
        /// <returns>Un objeto de tipo <see cref="User"/> creado con los datos del login.</returns>
        public static User ToUser(this LoginRequest request)
        {
            User newUser = new User();
            {
                newUser.Dni = request.Dni;
                newUser.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
                return newUser;
            }
        }

        /// <summary>
        /// Mapea un objeto de tipo <see cref="CreateUserRequest"/> a un objeto de tipo <see cref="User"/>.
        /// </summary>
        /// <param name="request">El objeto de tipo <see cref="CreateUserRequest"/> que contiene los datos para crear un usuario.</param>
        /// <returns>Un objeto de tipo <see cref="User"/> creado con los datos proporcionados.</returns>
        /// <exception cref="InvalidRoleException">Lanzada cuando el rol especificado no es válido.</exception>
        public static User toUser(this CreateUserRequest request)
        {
            if (Enum.TryParse<Role>(request.Role.Trim(), true, out var userRole))
            {
                User newUser = new User();
                {
                    newUser.Dni = request.Dni;
                    newUser.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
                    newUser.Role = userRole;
                    return newUser;
                }
            }
            else
            {
                throw new InvalidRoleException(request.Role);
            }
        }

        /// <summary>
        /// Mapea un objeto de tipo <see cref="UserResponse"/> a un objeto de tipo <see cref="User"/>.
        /// </summary>
        /// <param name="user">El objeto de tipo <see cref="UserResponse"/> con los datos del usuario.</param>
        /// <returns>Un objeto de tipo <see cref="User"/> con los datos mapeados desde el response.</returns>
        public static User ToUser(this UserResponse user)
        {
            User newUser = new User();
            {
                newUser.Id = user.Id;
                newUser.Dni = user.Dni;
                newUser.Role = (Role) Enum.Parse(typeof(Role), user.Role);
                newUser.CreatedAt = user.CreatedAt;
                newUser.UpdatedAt = user.UpdatedAt;
                newUser.IsDeleted = user.IsDeleted;
                return newUser;
            }
        }
    }
}
