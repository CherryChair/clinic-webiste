import React from 'react';
import { Navigate } from 'react-router-dom';
import Cookies from 'js-cookie';
import 'jwt-decode'
 
const RouteGuard = ({ component: Component, ...rest }) => {
   return Cookies.get("token") ? <Component {...rest}/> : <Navigate to={{pathname: '/login'}} replace/>; 
};
 
export default RouteGuard;