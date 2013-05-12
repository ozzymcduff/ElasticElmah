

namespace Elmah
{
    #region Imports
    
    using System;

    using SerializationInfo = System.Runtime.Serialization.SerializationInfo;
    using StreamingContext = System.Runtime.Serialization.StreamingContext;

    #endregion

    /// <summary>
    /// The exception that is thrown when to test the error logging 
    /// subsystem. This exception is used for testing purposes only and 
    /// should not be used for any other purpose.
    /// </summary>
    
    [ Serializable ]
    public sealed class TestException : System.ApplicationException
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="TestException"/> class.
        /// </summary>

        public TestException() : 
            this("This is a test exception that can be safely ignored.") {}
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TestException"/> class 
        /// with a specified error message.
        /// </summary>

        public TestException(string message) : 
            base(message) {}
        
        /// <summary>
        /// ializes a new instance of the <see cref="TestException"/> 
        /// class with a specified error message and a reference to the 
        /// inner exception that is the cause of this exception.
        /// </summary>

        public TestException(string message, Exception innerException) : 
            base(message, innerException) {}
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TestException"/> class 
        /// with serialized data.
        /// </summary>

        private TestException(SerializationInfo info, StreamingContext context) : 
            base(info, context) {}
    }
}

