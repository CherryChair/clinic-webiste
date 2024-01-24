import axios from 'axios';
import Cookies from 'js-cookie';
import { jwtDecode } from 'jwt-decode';

const decodeTokenAndSetRole = (token) => {
    let decodedToken = jwtDecode(token);
    decodedToken.IsPatient ? localStorage.setItem("isPatient", true) : localStorage.setItem("isPatient", false);
    decodedToken.IsDoctor ? localStorage.setItem("isDoctor", true) : localStorage.setItem("isDoctor", false);
    decodedToken.IsAdmin ? localStorage.setItem("isAdmin", true) : localStorage.setItem("isAdmin", false);
}

export const isPatient = () => {
    return localStorage.getItem("isPatient");
}

export const isDoctor = () => {
    return localStorage.getItem("isDoctor");
}

export const isAdmin = () => {
    return localStorage.getItem("isAdmin");
}

export const isLoggedIn = () => {
    return Cookies.get("token") ? true : false;
}

export const setAuthToken = token => {
   if (token) {
       axios.defaults.headers.common["Authorization"] = `Bearer ${token}`;
   }
   else
       delete axios.defaults.headers.common["Authorization"];
}

const handleSubmit = event => {
    //reqres registered sample user
    event.preventDefault();
    const loginPayload = {
      email: event.target.email.value,
      password: event.target.password.value
    }
    axios.post("https://localhost:7298/Patients/login", loginPayload)
      .then(response => {
        //get token from response
        const token  =  response.data.token;

        decodeTokenAndSetRole(token);
  
        //set JWT token to local
        Cookies.set("token", token, {expires: Date.parse(response.data.expiration)});
        //set token to axios common header
        setAuthToken(token);
  
 //redirect user to home page
        window.location.href = '/'
      })
      .catch(err => console.log(err));
    };
 


function LoginPage() {
    return (
        <>
          {/*
            This example requires updating your template:
    
            ```
            <html class="h-full bg-white">
            <body class="h-full">
            ```
          */}
          <div className="flex min-h-full flex-1 flex-col justify-center px-6 py-12 lg:px-8">
            <div className="sm:mx-auto sm:w-full sm:max-w-sm">
              <img
                className="mx-auto h-10 w-auto"
                src="https://tailwindui.com/img/logos/mark.svg?color=indigo&shade=600"
                alt="Your Company"
              />
              <h2 className="mt-10 text-center text-2xl font-bold leading-9 tracking-tight text-gray-900">
                Sign in to your account
              </h2>
            </div>
    
            <div className="mt-10 sm:mx-auto sm:w-full sm:max-w-sm">
              {/* <form className="space-y-6" action="#" method="POST"> */}
              <form className="space-y-6" onSubmit={handleSubmit}>
                <div>
                  <div className="flex items-center justify-between">
                    <label htmlFor="email" className="block text-sm font-medium leading-6 text-gray-900">
                        Email address
                    </label>
                  </div>
                  <div className="mt-2">
                    <input
                      id="email"
                      name="email"
                      type="email"
                      autoComplete="email"
                      required
                      className="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6"
                    />
                  </div>
                </div>
    
                <div>
                  <div className="flex items-center justify-between">
                    <label htmlFor="password" className="block text-sm font-medium leading-6 text-gray-900">
                      Password
                    </label>
                  </div>
                  <div className="mt-2">
                    <input
                      id="password"
                      name="password"
                      type="password"
                      autoComplete="current-password"
                      required
                      className="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6"
                    />
                  </div>
                </div>
    
                <div>
                  <button
                    type="submit"
                    className="flex w-full justify-center rounded-md bg-indigo-600 px-3 py-1.5 text-sm font-semibold leading-6 text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
                  >
                    Sign in
                  </button>
                </div>
              </form>
    
              <p className="mt-10 text-center text-sm text-gray-500">
                Not a member?{' '}
                <a href="/register" className="font-semibold leading-6 text-indigo-600 hover:text-indigo-500">
                  Register
                </a>
              </p>
            </div>
          </div>
        </>
      )
 }
 
 export default LoginPage;