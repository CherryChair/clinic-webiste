import Cookies from 'js-cookie';
import { setAuthToken } from '../pages/Login';

const handleClick = () => {
    Cookies.remove("token");
    setAuthToken();
    window.location.href = '/';
};

function Logout() {
    return (
      <button onClick={handleClick} class="block py-2 px-3 text-gray-900 rounded hover:bg-gray-100 md:hover:bg-transparent md:border-0 md:hover:text-blue-700 md:p-0 dark:text-white md:dark:hover:text-blue-500 dark:hover:bg-gray-700 dark:hover:text-white md:dark:hover:bg-transparent">Logout</button>
    );
 }
 
 export default Logout;