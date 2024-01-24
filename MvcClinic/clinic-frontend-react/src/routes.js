import React from "react";
import { Navigate, Routes, Route, BrowserRouter } from "react-router-dom";
import RouteGuard from "./components/RouteGuard"

//history
import { history } from './helpers/history';
 
//pages
import HomePage from "./pages/Home"
import LoginPage from "./pages/Login"
 
function MyRoutes() {
   return (
       <BrowserRouter history={history}>
           <Routes>
                <Route path="/home" element={<RouteGuard component={HomePage}/>}/>
               <Route
                   path="/login"
                   element={<LoginPage/>}
               />
               <Route
                    path="*"
                    element={<Navigate to="/home" replace />}
                />
               {/* <Navigate to="/" replace/> */}
           </Routes>
       </BrowserRouter>
   );
}
 
export default MyRoutes