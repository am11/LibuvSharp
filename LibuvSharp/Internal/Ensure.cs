using System;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	internal class Ensure
	{
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern uv_err_t uv_last_error(LoopSafeHandle loop);

		internal static Exception Map(uv_err_t error, string name = null)
		{
			if (error.code == uv_err_code.UV_OK) {
				return null;
			}

			switch (error.code) {
			case uv_err_code.UV_EINVAL:
				return new ArgumentException(error.Description);
			case uv_err_code.UV_ENOENT:
				var path = (name == null ? System.IO.Directory.GetCurrentDirectory() : Path.Combine(System.IO.Directory.GetCurrentDirectory(), name));
				return new System.IO.FileNotFoundException(string.Format("Could not find file '{0}'.", path), path);
			case uv_err_code.UV_EADDRINUSE:
				return new SocketException(10048);
			case uv_err_code.UV_EADDRNOTAVAIL:
				return new SocketException(10049);
			case uv_err_code.UV_ECONNREFUSED:
				return new SocketException(10061);
			default:
				return new UVException(error);
			}
		}

		internal static void Success(uv_err_t error)
		{
			var e = Map(error);
			if (e != null) {
				throw e;
			}
		}

		internal static void Success(int errorCode, Loop loop)
		{
			if (errorCode < 0) {
				var ex = Success(loop);
				if (ex != null) {
					throw ex;
				}
			}
		}

		internal static void Success(int errorCode, Loop loop, Action<Exception> callback, string name = null)
		{
			if (callback != null) {
				callback(errorCode < 0 ? Success(loop, null) : null);
			}
		}

		internal static void Success<T>(Exception ex, Action<Exception, T> callback, T arg)
		{
			if (callback != null) {
				callback(ex, arg);
			}
		}

		internal static Exception Success(Loop loop, string name = null)
		{
			return Map(uv_last_error(loop.NativeHandle), name);
		}

		public static void ArgumentNotNull(object argumentValue, string argumentName)
		{
			if (argumentValue == null) {
				throw new ArgumentNullException(argumentName);
			}
		}

		public static void AddressFamily(IPAddress ipAddress)
		{
			if ((ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) &&
			    (ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)) {
				throw new ArgumentException("ipAddress has to be of AddressFamily InterNetwork or InterNetworkV6");
			}
		}
	}
}

