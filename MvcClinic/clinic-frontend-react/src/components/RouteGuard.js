import React from 'react';
import { Navigate } from 'react-router-dom';
import 'jwt-decode'
import { isAdmin, isDoctor, isLoggedIn, isPatient } from '../pages/Login';
 
const RouteGuard = ({ adminComponent, doctorComponent, patientComponent, loggedIn, component: Component, ...rest }) => {
   let redirect = false;
   if(loggedIn && !isLoggedIn()) redirect = true;
   if(adminComponent && !isAdmin()) redirect = true;
   if(patientComponent && !isPatient()) redirect = true;
   if(doctorComponent && !isDoctor()) redirect = true;

   return redirect ? <Navigate to={{pathname: '/home'}} replace/> : <Component {...rest}/>; 
};
 
export default RouteGuard;