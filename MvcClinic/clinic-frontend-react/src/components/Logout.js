import Cookies from 'js-cookie';
import { setAuthToken } from '../pages/Login';

const handleClick = () => {
    Cookies.remove("token");
    setAuthToken();
};

function Logout({className}) {
    return (
      <a
        href="/home"
        className={className}
        onClick={handleClick}
        >Logout</a>
    );
 }


 
 export default Logout;