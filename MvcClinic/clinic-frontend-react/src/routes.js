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
import PatientEditPage from "./pages/PatientEditView";
import EmployeeListPage from "./pages/EmployeeList";
import EmployeeEditPage from "./pages/EmployeeEditView";
import ScheduleListPage from "./pages/ScheduleList";
 
function MyRoutes() {
   return (
       <BrowserRouter history={history}>
           <Routes>
                <Route path="/home" element={<HomePage/>}/>
                <Route
                    path="/login"
                    element={<RouteGuard notLoggedIn={true} component={LoginPage}/>}
                />
                <Route
                    path="/register"
                    element={<RouteGuard notLoggedIn={true} component={RegisterPage}/>}
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
                    path="/patient/:id"
                    element={<RouteGuard loggedIn={true} doctorComponent={true} adminComponent={true} component={PatientEditPage}/>}
                />
                <Route
                    path="/employees"
                    element={<RouteGuard loggedIn={true} adminComponent={true} component={EmployeeListPage}/>}
                />
                <Route
                    path="/employee/:id"
                    element={<RouteGuard loggedIn={true} adminComponent={true} component={EmployeeEditPage}/>}
                />
                <Route
                    path="/schedules"
                    element={<RouteGuard loggedIn={true} adminComponent={true} doctorComponent={true} patientComponent={true} component={ScheduleListPage}/>}
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