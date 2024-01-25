import React from "react";
import { Navigate, Routes, Route, BrowserRouter } from "react-router-dom";
import RouteGuard from "./components/RouteGuard"

//history
import { history } from './helpers/history';
 
//pages
import HomePage from "./pages/Home"
import LoginPage from "./pages/Login"
import RegisterPage from "./pages/Register";
import RegisterEmployeePage from "./pages/RegisterEmployee";
import PatientListPage from "./pages/PatientList";
 
function MyRoutes() {
   return (
       <BrowserRouter history={history}>
           <Routes>
                <Route path="/home" element={<HomePage/>}/>
                <Route
                    path="/login"
                    element={<LoginPage/>}
                />
                <Route
                    path="/register"
                    element={<RegisterPage/>}
                />
                <Route
                    path="/employee/register"
                    element={<RouteGuard loggedIn={true} adminComponent={true} component={RegisterEmployeePage}/>}
                />
                <Route
                    path="/patients"
                    element={<RouteGuard loggedIn={true} doctorComponent={true} adminComponent={true} component={PatientListPage}/>}
                />
                <Route
                    path="*"
                    element={<Navigate to="/home" replace />}
                />
           </Routes>
       </BrowserRouter>
   );
}
 
export default MyRoutes